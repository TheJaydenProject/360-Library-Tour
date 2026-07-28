using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PanoramaTourManager : MonoBehaviour
{
    [SerializeField] private MeshRenderer sphereRenderer;
    [SerializeField] private VRCamera playerCamera;
    [SerializeField] private List<PanoramaNode> nodes = new();

    [Header("Transition - Zoom Blur")]
    [SerializeField] private RadialZoomBlurController blurController;
    [SerializeField] private float punchInDuration = 0.2f;
    [SerializeField] private float punchOutDuration = 0.3f;
    [Range(0f, 1f)]
    [SerializeField] private float maxBlurStrength = 1f;

    [Header("Transition - Screen Flash (hides the swap frame)")]
    [Tooltip("Full-screen black Image used to mask the panorama swap")]
    [SerializeField] private Image transitionOverlay;
    [Range(0f, 1f)]
    [SerializeField] private float maxOverlayAlpha = 0.35f;

    private bool _isTransitioning;
    private bool _isLoading;
    private readonly HashSet<VideoPlayer> _primedVideoPlayers = new();

    public IReadOnlyList<PanoramaNode> Nodes => nodes;
    public VRCamera PlayerCamera => playerCamera;

#if UNITY_EDITOR
    private string _previewedNodeIdInEditor;
    private ArrivalDirection _previewedDirectionInEditor;
#endif

    // Editor-only convenience: swap the sphere to a node's material and activate only its
    // hotspot group, so a room can be previewed and edited in the Scene view without Play mode.
    public void PreviewNodeInEditor(string nodeId, ArrivalDirection direction = ArrivalDirection.In)
    {
#if UNITY_EDITOR
        _previewedNodeIdInEditor = nodeId;
        _previewedDirectionInEditor = direction;
#endif

        PanoramaNode targetNode = nodes.Find(n => n.nodeId == nodeId);
        if (targetNode == null || sphereRenderer == null) return;

        sphereRenderer.sharedMaterial = targetNode.panoramaMaterial;

        foreach (PanoramaNode node in nodes)
        {
            if (node.hotspotGroup != null)
            {
                node.hotspotGroup.SetActive(node.nodeId == nodeId);
            }
        }

        if (playerCamera != null)
        {
            (float rotY, float rotX) = GetArrivalRotation(targetNode, direction);
            playerCamera.SetLookDirection(rotY, rotX);
        }
    }

#if UNITY_EDITOR
    // Once a node has been previewed via the button, keep it in sync automatically: any further
    // edit to its fields (material, rotation, hotspot group) re-applies live instead of requiring
    // another manual click. SetActive can't be called directly inside OnValidate (Unity restricts
    // SendMessage calls there), so the actual refresh is deferred to the next editor tick.
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_previewedNodeIdInEditor)) return;

        string nodeId = _previewedNodeIdInEditor;
        ArrivalDirection direction = _previewedDirectionInEditor;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            PreviewNodeInEditor(nodeId, direction);
        };
    }
#endif

    // Out-facing rotation is optional per node - falls back to the In-facing rotation when a
    // node has only one direction configured, so most nodes never need the Out fields filled in.
    private static (float rotationY, float rotationX) GetArrivalRotation(PanoramaNode node, ArrivalDirection direction)
    {
        if (direction == ArrivalDirection.Out && node.useSeparateOutFacing)
        {
            return (node.outCameraRotationY, node.outCameraRotationX);
        }

        return (node.cameraRotationY, node.cameraRotationX);
    }

    private void Start()
    {
        if (nodes.Count == 0)
        {
            Debug.LogError($"{nameof(PanoramaTourManager)} has no nodes assigned.");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError($"{nameof(PanoramaTourManager)} is missing a player camera reference.");
            return;
        }

        if (blurController != null)
        {
            blurController.SetBlurStrength(0f);
        }

        ActivateNode(nodes[0].nodeId, applyFacing: false);

        StartCoroutine(LoadAllVideosThenReveal());
    }

    // Keeps the tour hidden behind the transition overlay and blocks navigation until every
    // video node has actually finished preparing and shown its first frame, so nobody ever sees
    // a black room mid-tour - the one-time cost happens up front instead of scattered around.
    private IEnumerator LoadAllVideosThenReveal()
    {
        _isLoading = true;
        SetOverlayAlpha(1f);

        foreach (PanoramaNode node in nodes)
        {
            if (node.videoPlayer != null && !node.videoPlayer.isPrepared)
            {
                node.videoPlayer.Prepare();
            }
        }

        foreach (PanoramaNode node in nodes)
        {
            if (node.videoPlayer == null) continue;

            yield return new WaitUntil(() => node.videoPlayer.isPrepared);
            ShowFirstVideoFrameOnly(node.videoPlayer);
        }

        _isLoading = false;

        yield return AnimateTransitionPhase(0f, 0f, 1f, 0f, punchOutDuration);
    }

    public void NavigateToNode(string nodeId, ArrivalDirection direction = ArrivalDirection.In)
    {
        // Ignore clicks that arrive mid-transition, or before the initial load has finished
        if (_isTransitioning || _isLoading) return;

        StartCoroutine(TransitionToNode(nodeId, direction));
    }

    private IEnumerator TransitionToNode(string nodeId, ArrivalDirection direction)
    {
        _isTransitioning = true;

        yield return AnimateTransitionPhase(0f, maxBlurStrength, 0f, maxOverlayAlpha, punchInDuration);

        // Swap happens at peak blur, hidden inside the heaviest distortion
        ActivateNode(nodeId, applyFacing: true, direction);

        yield return AnimateTransitionPhase(maxBlurStrength, 0f, maxOverlayAlpha, 0f, punchOutDuration);

        _isTransitioning = false;
    }

    private IEnumerator AnimateTransitionPhase(float fromBlur, float toBlur, float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t); // smoothstep easing

            if (blurController != null)
            {
                blurController.SetBlurStrength(Mathf.Lerp(fromBlur, toBlur, eased));
            }

            SetOverlayAlpha(Mathf.Lerp(fromAlpha, toAlpha, eased));

            yield return null;
        }

        blurController?.SetBlurStrength(toBlur);
        SetOverlayAlpha(toAlpha);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (transitionOverlay == null) return;

        Color color = transitionOverlay.color;
        color.a = alpha;
        transitionOverlay.color = color;
    }

    private void ActivateNode(string nodeId, bool applyFacing, ArrivalDirection direction = ArrivalDirection.In)
    {
        PanoramaNode targetNode = nodes.Find(n => n.nodeId == nodeId);

        if (targetNode == null)
        {
            Debug.LogError($"No node found with id '{nodeId}'.");
            return;
        }

        if (sphereRenderer == null || targetNode.panoramaMaterial == null)
        {
            Debug.LogError($"Node '{nodeId}' is missing a sphere renderer or panorama material.");
            return;
        }

        sphereRenderer.material = targetNode.panoramaMaterial;

        foreach (PanoramaNode node in nodes)
        {
            if (node.hotspotGroup != null)
            {
                node.hotspotGroup.SetActive(node.nodeId == nodeId);
            }
        }

        if (applyFacing && playerCamera != null)
        {
            (float rotY, float rotX) = GetArrivalRotation(targetNode, direction);
            playerCamera.SetLookDirection(rotY, rotX);
        }

        if (targetNode.videoPlayer != null)
        {
            ShowFirstVideoFrameOnly(targetNode.videoPlayer);
        }
    }

    // Renders exactly one frame into the VideoPlayer's render texture and leaves it paused there,
    // instead of looping - gives a "living" first frame without actual playback. Only primes once
    // per VideoPlayer for the whole session, so revisiting the node later doesn't advance it further.
    // Play() then Pause() is used instead of StepForward() alone, since StepForward on a player that
    // has never actually played doesn't reliably push a frame into the render target on every backend.
    private void ShowFirstVideoFrameOnly(VideoPlayer videoPlayer)
    {
        if (_primedVideoPlayers.Contains(videoPlayer)) return;
        _primedVideoPlayers.Add(videoPlayer);

        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
            videoPlayer.Pause();
            return;
        }

        void OnPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnPrepared;
            vp.Play();
            vp.Pause();
        }

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();
    }
}
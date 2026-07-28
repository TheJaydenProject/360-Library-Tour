using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

[CustomEditor(typeof(PanoramaTourManager))]
public class PanoramaTourManagerEditor : Editor
{
    private VideoPlayer _pendingPreviewVideoPlayer;

    // Kicks off Prepare() for every video node as soon as TourManager is selected, so by the
    // time you click between Preview buttons most are already loaded instead of each click
    // hitting its own wait.
    private void OnEnable()
    {
        var manager = target as PanoramaTourManager;
        if (manager == null) return;

        foreach (PanoramaNode node in manager.Nodes)
        {
            if (node.videoPlayer != null && !node.videoPlayer.isPrepared)
            {
                node.videoPlayer.Prepare();
            }
        }
    }

    private void OnDisable()
    {
        if (_pendingPreviewVideoPlayer != null)
        {
            _pendingPreviewVideoPlayer.prepareCompleted -= OnVideoPrepared;
            _pendingPreviewVideoPlayer = null;
        }
    }

    public override void OnInspectorGUI()
    {
        var manager = (PanoramaTourManager)target;

        EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);

        foreach (PanoramaNode node in manager.Nodes)
        {
            if (string.IsNullOrEmpty(node.nodeId)) continue;

            string roomLabel = node.panoramaMaterial != null
                ? $"{node.nodeId} — {node.panoramaMaterial.name}"
                : node.nodeId;

            // The Out button only appears once a node actually has a separate out-facing rotation
            // configured, since for most nodes In and Out are identical - no point doubling every button.
            if (node.useSeparateOutFacing)
            {
                if (GUILayout.Button($"Preview '{roomLabel}' (In)"))
                {
                    PreviewNode(manager, node, ArrivalDirection.In);
                }

                if (GUILayout.Button($"Preview '{roomLabel}' (Out)"))
                {
                    PreviewNode(manager, node, ArrivalDirection.Out);
                }
            }
            else if (GUILayout.Button($"Preview '{roomLabel}'"))
            {
                PreviewNode(manager, node, ArrivalDirection.In);
            }
        }

        EditorGUILayout.Space();

        DrawDefaultInspector();
    }

    private void PreviewNode(PanoramaTourManager manager, PanoramaNode node, ArrivalDirection direction)
    {
        Undo.RecordObject(manager, "Preview Panorama Node");

        manager.PreviewNodeInEditor(node.nodeId, direction);

        foreach (PanoramaNode n in manager.Nodes)
        {
            if (n.hotspotGroup != null)
            {
                EditorUtility.SetDirty(n.hotspotGroup);
            }
        }

        // Snap the Scene view to match the player camera so it shows the same facing
        // as the Game tab, instead of staying wherever it last happened to be pointed.
        if (manager.PlayerCamera != null && SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.AlignViewToObject(manager.PlayerCamera.transform);
        }

        if (node.videoPlayer != null)
        {
            RequestVideoPreviewFrame(node.videoPlayer);
        }
    }

    // Video preparation is async even in Edit mode, so this plays-then-pauses a single decoded
    // frame into the VideoPlayer's render texture once ready, giving a non-black preview of a
    // video room. Play()+Pause() is used instead of StepForward() alone, since StepForward on a
    // player that has never actually played doesn't reliably push a frame into the render target.
    private void RequestVideoPreviewFrame(VideoPlayer videoPlayer)
    {
        if (_pendingPreviewVideoPlayer != null)
        {
            _pendingPreviewVideoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
            videoPlayer.Pause();
            EditorApplication.update += RepaintOnce;
        }
        else
        {
            _pendingPreviewVideoPlayer = videoPlayer;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnVideoPrepared;
        _pendingPreviewVideoPlayer = null;
        vp.Play();
        vp.Pause();
        EditorApplication.update += RepaintOnce;
    }

    private void RepaintOnce()
    {
        EditorApplication.update -= RepaintOnce;

        // SceneView.RepaintAll() alone doesn't refresh the Game tab in Edit mode - it needs an
        // explicit nudge, otherwise the frame is actually ready but the Game view just won't
        // redraw itself until you click into it manually.
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}

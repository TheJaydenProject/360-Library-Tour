using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

[CustomEditor(typeof(PanoramaTourManager))]
public class PanoramaTourManagerEditor : Editor
{
    private VideoPlayer _pendingPreviewVideoPlayer;

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

    // Video preparation is async even in Edit mode, so this steps a single decoded frame into
    // the VideoPlayer's render texture once ready, giving a non-black preview of a video room.
    private void RequestVideoPreviewFrame(VideoPlayer videoPlayer)
    {
        if (_pendingPreviewVideoPlayer != null)
        {
            _pendingPreviewVideoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        if (videoPlayer.isPrepared)
        {
            videoPlayer.StepForward();
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
        vp.StepForward();
        EditorApplication.update += RepaintOnce;
    }

    private void RepaintOnce()
    {
        EditorApplication.update -= RepaintOnce;
        SceneView.RepaintAll();
    }
}

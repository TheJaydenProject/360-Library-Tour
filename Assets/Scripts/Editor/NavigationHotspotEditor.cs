using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavigationHotspot))]
public class NavigationHotspotEditor : Editor
{
    // Index 0 in the dropdown is always the "-- Select --" placeholder prepended to the node list.
    private const int NoSelectionIndex = 0;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "m_Script", "targetNodeId");

        SerializedProperty tourManagerProp = serializedObject.FindProperty("tourManager");
        SerializedProperty targetNodeIdProp = serializedObject.FindProperty("targetNodeId");
        var tourManager = tourManagerProp.objectReferenceValue as PanoramaTourManager;

        if (tourManager == null)
        {
            EditorGUILayout.PropertyField(targetNodeIdProp);
            EditorGUILayout.HelpBox("Assign a Tour Manager above to pick the target node from a dropdown.", MessageType.Info);
        }
        else
        {
            PanoramaNode[] availableNodes = tourManager.Nodes
                .Where(n => !string.IsNullOrEmpty(n.nodeId))
                .ToArray();

            string[] nodeIds = availableNodes.Select(n => n.nodeId).ToArray();

            // Show the material name alongside the id so it's recognizable as a room, not just a number.
            string[] labels = availableNodes
                .Select(n => n.panoramaMaterial != null ? $"{n.nodeId} — {n.panoramaMaterial.name}" : n.nodeId)
                .ToArray();

            string[] options = new[] { "-- Select --" }.Concat(labels).ToArray();
            int currentIndex = Array.IndexOf(nodeIds, targetNodeIdProp.stringValue);
            int displayIndex = currentIndex < 0 ? NoSelectionIndex : currentIndex + 1;

            EditorGUI.BeginChangeCheck();
            int selected = EditorGUILayout.Popup("Target Node Id", displayIndex, options);
            if (EditorGUI.EndChangeCheck())
            {
                targetNodeIdProp.stringValue = selected == NoSelectionIndex ? string.Empty : nodeIds[selected - 1];
            }

            if (currentIndex < 0 && !string.IsNullOrEmpty(targetNodeIdProp.stringValue))
            {
                EditorGUILayout.HelpBox(
                    $"Stored target '{targetNodeIdProp.stringValue}' doesn't match any node on the Tour Manager.",
                    MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}

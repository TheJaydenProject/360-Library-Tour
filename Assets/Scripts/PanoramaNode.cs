using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class PanoramaNode
{
    [Tooltip("Unique identifier for this node, referenced by hotspots to navigate here")]
    public string nodeId;

    [Tooltip("Panorama material applied to the sphere when this node is active")]
    public Material panoramaMaterial;

    [Tooltip("Parent GameObject holding this node's hotspots")]
    public GameObject hotspotGroup;

    [Tooltip("Optional: VideoPlayer driving this node's material, used to render a first-frame preview in the editor")]
    public VideoPlayer videoPlayer;

    [Header("Arrival Camera Rotation (In)")]
    [Tooltip("Copy this from the Main Camera's Transform > Rotation > X value while previewing this node")]
    public float cameraRotationX;

    [Tooltip("Copy this from the Main Camera's Transform > Rotation > Y value while previewing this node")]
    public float cameraRotationY;

    [Header("Arrival Camera Rotation (Out) - optional")]
    [Tooltip("Enable if hotspots leaving this node should arrive facing a different direction than hotspots entering it")]
    public bool useSeparateOutFacing;

    [Tooltip("Only used when 'Use Separate Out Facing' is checked")]
    public float outCameraRotationX;

    [Tooltip("Only used when 'Use Separate Out Facing' is checked")]
    public float outCameraRotationY;
}
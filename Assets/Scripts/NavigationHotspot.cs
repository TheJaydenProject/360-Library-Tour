using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class NavigationHotspot : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Identifier for debugging/reference, not used for navigation logic")]
    [SerializeField] private string hotspotId;

    [SerializeField] private PanoramaTourManager tourManager;

    [Tooltip("The nodeId of the destination this hotspot leads to")]
    [SerializeField] private string targetNodeId;

    [Tooltip("Which of the target node's arrival rotations to use (only matters if that node has 'Use Separate Out Facing' enabled)")]
    [SerializeField] private ArrivalDirection arrivalDirection = ArrivalDirection.In;

    [Tooltip("If set, updates the HUD objective text when this hotspot is clicked. Leave blank for hotspots that shouldn't change it (most navigation hotspots).")]
    [TextArea(2, 4)]
    [SerializeField] private string objectiveTextOnArrival;

    public string TargetNodeId => targetNodeId;
    public ArrivalDirection ArrivalDirection => arrivalDirection;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tourManager == null || string.IsNullOrEmpty(targetNodeId))
        {
            Debug.LogError($"Hotspot '{hotspotId}' on {gameObject.name} is missing a tour manager or target node id.");
            return;
        }

        tourManager.NavigateToNode(targetNodeId, arrivalDirection);

        if (!string.IsNullOrEmpty(objectiveTextOnArrival) && UIManager.Instance != null)
        {
            UIManager.Instance.SetObjectiveText(objectiveTextOnArrival);
        }
    }
}
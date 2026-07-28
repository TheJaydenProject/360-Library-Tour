using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class VRCamera : MonoBehaviour
{
    private const float FullCircleDegrees = 360f;
    private const float HalfCircleDegrees = 180f;

    [Header("Look Sensitivity")]
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float minPitch = -89f;
    [SerializeField] private float maxPitch = 89f;

    private Camera _cam;
    private bool _isDragging;
    private Vector2 _lastMousePosition;
    private float _yaw;
    private float _pitch;
    public Camera Cam => _cam;

    private void Start()
    {
        if (!TryGetComponent(out _cam))
        {
            Debug.LogError($"{nameof(VRCamera)} requires a Camera component on {gameObject.name}. Disabling script.");
            enabled = false;
            return;
        }

        Vector3 currentEuler = transform.eulerAngles;
        _yaw = currentEuler.y;
        _pitch = NormalizePitch(currentEuler.x);
    }

    private void Update()
    {
        // Mouse.current is null until a mouse device is detected (e.g. touch-only platforms)
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame && !_isDragging)
        {
            _isDragging = true;
            _lastMousePosition = mouse.position.ReadValue();
        }
        else if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
        {
            _isDragging = false;
        }
    }

    private void LateUpdate()
    {
        if (!_isDragging) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 currentMousePosition = mouse.position.ReadValue();
        Vector2 delta = currentMousePosition - _lastMousePosition;

        _yaw += delta.x * sensitivity;
        _pitch -= delta.y * sensitivity; // inverted so dragging up tilts the view up
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        _lastMousePosition = currentMousePosition;
    }

    // Called externally (e.g. by a hotspot) to snap the view to a specific facing on arrival at a new node
    public void SetLookDirection(float yaw, float pitch)
    {
        _yaw = yaw;
        _pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // Converts a 0-360 euler X angle to -180/180 so Mathf.Clamp behaves correctly
    private static float NormalizePitch(float eulerX)
    {
        return eulerX > HalfCircleDegrees ? eulerX - FullCircleDegrees : eulerX;
    }
}
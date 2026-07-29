using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [SerializeField] private float targetFps = 60f;
    [SerializeField] private float checkInterval = 3f;

    private float _accumulatedTime;
    private int _frameCount;

    private void Update()
    {
        _accumulatedTime += Time.unscaledDeltaTime;
        _frameCount++;

        if (_accumulatedTime < checkInterval) return;

        float fps = _frameCount / _accumulatedTime;
        if (fps < targetFps)
        {
            Debug.LogWarning($"FPS dropped below {targetFps}: {fps:F1}");
        }

        _accumulatedTime = 0f;
        _frameCount = 0;
    }
}

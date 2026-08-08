using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Requires an EventSystem and a PhysicsRaycaster on the camera in the scene
// so pointer events reach this object's Collider.
[RequireComponent(typeof(Collider))]
public class HotspotVisual : MonoBehaviour, IPointerClickHandler
{
    [Header("Idle Pulse")]
    [SerializeField] private float idlePulseSpeed = 2f;
    [SerializeField] private float idlePulseMinScale = 1f;
    [SerializeField] private float idlePulseMaxScale = 1.1f;

    [Header("Text Panel")]
    [SerializeField] private CanvasGroup textPanel;
    [SerializeField] private float panelAnimationDuration = 0.33f;

    private Vector3 _baseScale;
    private bool _isPanelOpen;
    private Coroutine _panelCoroutine;

    private void Awake()
    {
        _baseScale = transform.localScale;

        if (textPanel != null)
        {
            textPanel.alpha = 0f;
            textPanel.interactable = false;
            textPanel.blocksRaycasts = false;
            textPanel.transform.localScale = Vector3.zero;
        }
    }

    private void Update()
    {
        float wave = (Mathf.Sin(Time.time * idlePulseSpeed) + 1f) * 0.5f;
        float pulse = Mathf.Lerp(idlePulseMinScale, idlePulseMaxScale, wave);
        transform.localScale = _baseScale * pulse;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (textPanel == null)
        {
            return;
        }

        _isPanelOpen = !_isPanelOpen;

        if (_panelCoroutine != null)
        {
            StopCoroutine(_panelCoroutine);
        }

        _panelCoroutine = StartCoroutine(AnimatePanel(_isPanelOpen));
    }

    private IEnumerator AnimatePanel(bool open)
    {
        Transform panelTransform = textPanel.transform;
        float startAlpha = textPanel.alpha;
        Vector3 startScale = panelTransform.localScale;
        float targetAlpha = open ? 1f : 0f;
        Vector3 targetScale = open ? Vector3.one : Vector3.zero;

        if (open)
        {
            textPanel.interactable = true;
            textPanel.blocksRaycasts = true;
        }

        float elapsed = 0f;
        while (elapsed < panelAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / panelAnimationDuration);
            textPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            panelTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        textPanel.alpha = targetAlpha;
        panelTransform.localScale = targetScale;

        if (!open)
        {
            textPanel.interactable = false;
            textPanel.blocksRaycasts = false;
        }

        _panelCoroutine = null;
    }
}

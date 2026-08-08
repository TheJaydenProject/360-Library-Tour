using System.Collections;
using TMPro;
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
    [SerializeField] private Canvas textPanel;
    [SerializeField] private float panelAnimationDuration = 0.33f;

    [Header("Title & Voiceover")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private AudioSource voiceoverSource;
    [SerializeField] private AudioClip voiceoverClip;
    [SerializeField] private float titleTypeDuration = 1f;
    [SerializeField] private float bodyTypeSpeedMultiplier = 1.1f;

    private Vector3 _baseScale;
    private Vector3 _panelOpenScale;
    private CanvasGroup _panelCanvasGroup;
    private bool _isPanelOpen;
    private Coroutine _panelCoroutine;

    private string _titleFullText;
    private string _bodyFullText;
    private Coroutine _sequenceCoroutine;

    private void Awake()
    {
        _baseScale = transform.localScale;

        if (textPanel != null)
        {
            _panelOpenScale = textPanel.transform.localScale;

            _panelCanvasGroup = textPanel.GetComponent<CanvasGroup>();
            if (_panelCanvasGroup == null)
            {
                _panelCanvasGroup = textPanel.gameObject.AddComponent<CanvasGroup>();
            }

            _panelCanvasGroup.alpha = 0f;
            _panelCanvasGroup.interactable = false;
            _panelCanvasGroup.blocksRaycasts = false;
            textPanel.transform.localScale = Vector3.zero;
        }

        if (titleText != null)
        {
            _titleFullText = titleText.text;
            titleText.text = "";
        }

        if (bodyText != null)
        {
            _bodyFullText = bodyText.text;
            bodyText.text = "";
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

        if (_isPanelOpen)
        {
            ReplaySequence();
        }
        else
        {
            StopSequence();
        }
    }

    // Wire this to the panel's replay button.
    public void ReplaySequence()
    {
        StopSequence();
        _sequenceCoroutine = StartCoroutine(PlayTitleThenBodySequence());
    }

    private void StopSequence()
    {
        if (_sequenceCoroutine != null)
        {
            StopCoroutine(_sequenceCoroutine);
            _sequenceCoroutine = null;
        }

        if (voiceoverSource != null)
        {
            voiceoverSource.Stop();
        }

        if (titleText != null)
        {
            titleText.text = "";
        }

        if (bodyText != null)
        {
            bodyText.text = "";
        }
    }

    private IEnumerator PlayTitleThenBodySequence()
    {
        yield return TypeText(titleText, _titleFullText, titleTypeDuration);

        if (voiceoverSource != null && voiceoverClip != null)
        {
            voiceoverSource.clip = voiceoverClip;
            voiceoverSource.Play();
            yield return TypeText(bodyText, _bodyFullText, voiceoverClip.length / bodyTypeSpeedMultiplier);
        }

        _sequenceCoroutine = null;
    }

    private IEnumerator TypeText(TMP_Text label, string fullText, float duration)
    {
        if (label == null || string.IsNullOrEmpty(fullText))
        {
            yield break;
        }

        label.text = "";
        float delayPerCharacter = duration / fullText.Length;

        foreach (char character in fullText)
        {
            label.text += character;
            yield return new WaitForSeconds(delayPerCharacter);
        }
    }

    private IEnumerator AnimatePanel(bool open)
    {
        Transform panelTransform = textPanel.transform;
        float startAlpha = _panelCanvasGroup.alpha;
        Vector3 startScale = panelTransform.localScale;
        float targetAlpha = open ? 1f : 0f;
        Vector3 targetScale = open ? _panelOpenScale : Vector3.zero;

        if (open)
        {
            _panelCanvasGroup.interactable = true;
            _panelCanvasGroup.blocksRaycasts = true;
        }

        float elapsed = 0f;
        while (elapsed < panelAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / panelAnimationDuration);
            _panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            panelTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        _panelCanvasGroup.alpha = targetAlpha;
        panelTransform.localScale = targetScale;

        if (!open)
        {
            _panelCanvasGroup.interactable = false;
            _panelCanvasGroup.blocksRaycasts = false;
        }

        _panelCoroutine = null;
    }
}

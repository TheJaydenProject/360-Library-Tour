using System.Collections;
using TMPro;
using UnityEngine;

public class HotspotInteraction : MonoBehaviour
{
    [Header("Canvas Setup")]
    [SerializeField] private CanvasGroup primaryCanvasGroup;

    [Header("DVD Content")]
    [SerializeField] private GameObject dvdGroup;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Hint Settings")]
    [TextArea]
    [SerializeField] private string subtitleMessage;

    [SerializeField] private float charactersPerSecond = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hintAudio;

    private Coroutine subtitleCoroutine;

    private void Awake()
    {
        ShowPrimaryCanvas();

        if (dvdGroup != null)
        {
            dvdGroup.SetActive(false);
        }

        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    // Connect this to the hotspot button's On Click().
    public void OpenHotspot()
    {
        StopHint();

        HidePrimaryCanvas();

        if (dvdGroup != null)
        {
            dvdGroup.SetActive(true);
        }

        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideUIGroup();
        }
    }

    // Connect this to the exit button's On Click().
    public void CloseHotspot()
    {
        StopHint();

        if (dvdGroup != null)
        {
            dvdGroup.SetActive(false);
        }

        ShowPrimaryCanvas();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowUIGroup();
        }
    }

    // Connect this to the hint button's On Click().
    public void PlayHint()
    {
        StopHint();

        subtitleCoroutine = StartCoroutine(PlayHintRoutine());
    }

    private IEnumerator PlayHintRoutine()
    {
        if (subtitleText == null)
        {
            yield break;
        }

        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        if (audioSource != null && hintAudio != null)
        {
            audioSource.clip = hintAudio;
            audioSource.Play();
        }

        float typingDelay = 1f / Mathf.Max(charactersPerSecond, 1f);

        foreach (char character in subtitleMessage)
        {
            subtitleText.text += character;
            yield return new WaitForSeconds(typingDelay);
        }

        // Small pause after typing finishes
        yield return new WaitForSeconds(1f);

        // Keep the completed subtitle visible until the audio finishes.
        if (audioSource != null && audioSource.isPlaying)
        {
            yield return new WaitUntil(() => !audioSource.isPlaying);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);
        subtitleCoroutine = null;
    }

    private void StopHint()
    {
        if (subtitleCoroutine != null)
        {
            StopCoroutine(subtitleCoroutine);
            subtitleCoroutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);
        }
    }

    private void HidePrimaryCanvas()
    {
        if (primaryCanvasGroup == null)
        {
            return;
        }

        primaryCanvasGroup.alpha = 0f;
        primaryCanvasGroup.interactable = false;
        primaryCanvasGroup.blocksRaycasts = false;
    }

    private void ShowPrimaryCanvas()
    {
        if (primaryCanvasGroup == null)
        {
            return;
        }

        primaryCanvasGroup.alpha = 1f;
        primaryCanvasGroup.interactable = true;
        primaryCanvasGroup.blocksRaycasts = true;
    }

    private void OnDisable()
    {
        StopHint();
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image startImage;
    [SerializeField] private Image mapImage;
    [SerializeField] private Image objectiveImage;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float objectiveImageDelay = 0.2f;
    [SerializeField] private float growShrinkDuration = 0.3f;

    [Header("Background Music")]
    [SerializeField] private AudioSource bgmAudioSource;

    private bool _isStartImageShown;
    private bool _isMapImageShown;

    private void Awake()
    {
        HideImmediately(startImage);
        HideImmediately(mapImage);
        HideImmediately(objectiveImage);
    }

    private void Start()
    {
        StartCoroutine(ShowStartImageAfterDelay());

        if (bgmAudioSource != null)
        {
            bgmAudioSource.Play();
        }
    }

    // Wire this to the music button's OnClick().
    public void ToggleMusicMute()
    {
        if (bgmAudioSource == null)
        {
            return;
        }

        bgmAudioSource.mute = !bgmAudioSource.mute;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (_isStartImageShown)
        {
            _isStartImageShown = false;
            StartCoroutine(Shrink(startImage));
            StartCoroutine(ShowObjectiveImageAfterDelay());
        }

        if (_isMapImageShown)
        {
            _isMapImageShown = false;
            StartCoroutine(Shrink(mapImage));
        }
    }

    private IEnumerator ShowStartImageAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        yield return Grow(startImage);
        _isStartImageShown = true;
    }

    private IEnumerator ShowObjectiveImageAfterDelay()
    {
        yield return new WaitForSeconds(objectiveImageDelay);
        yield return Grow(objectiveImage);
    }

    // Wire this to the map button's OnClick().
    public void ShowMapImage()
    {
        StartCoroutine(ShowMapImageRoutine());
    }

    private IEnumerator ShowMapImageRoutine()
    {
        yield return Grow(mapImage);
        _isMapImageShown = true;
    }

    private void HideImmediately(Image image)
    {
        if (image == null)
        {
            return;
        }

        image.transform.localScale = Vector3.zero;
        image.gameObject.SetActive(false);
    }

    private IEnumerator Grow(Image image)
    {
        if (image == null)
        {
            yield break;
        }

        image.gameObject.SetActive(true);
        yield return ScaleOverTime(image.transform, Vector3.zero, Vector3.one, growShrinkDuration);
    }

    private IEnumerator Shrink(Image image)
    {
        if (image == null)
        {
            yield break;
        }

        yield return ScaleOverTime(image.transform, Vector3.one, Vector3.zero, growShrinkDuration);
        image.gameObject.SetActive(false);
    }

    private IEnumerator ScaleOverTime(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        target.localScale = to;
    }
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Image startImage;
    [SerializeField] private Image mapImage;
    [SerializeField] private Image objectiveImage;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float objectiveImageDelay = 0.2f;
    [SerializeField] private float growShrinkDuration = 0.3f;

    [Header("Background Music")]
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("UI Group (hidden while a hotspot panel is open)")]
    [SerializeField] private GameObject uiGroup;

    [Header("Corner Solved Messages")]
    [TextArea(2, 4)]
    [SerializeField] private string oneCornerSolvedText = "1 of 3 corners solved. Find the next hidden corner.";
    [TextArea(2, 4)]
    [SerializeField] private string twoCornersSolvedText = "2 of 3 corners solved. One left to go.";
    [TextArea(2, 4)]
    [SerializeField] private string allCornersSolvedText = "All 3 corners solved. Tour complete!";

    private static int _cornersSolvedCount;

    private bool _isStartImageShown;
    private bool _isMapImageShown;

    private void Awake()
    {
        Instance = this;

        HideImmediately(startImage);
        HideImmediately(mapImage);
        HideImmediately(objectiveImage);
    }

    public void SetObjectiveText(string newText)
    {
        if (objectiveText != null)
        {
            objectiveText.text = newText;
        }
    }

    // Call this from a corner's riddle-solved success handler.
    public void ReportCornerSolved()
    {
        _cornersSolvedCount++;

        string message = _cornersSolvedCount switch
        {
            1 => oneCornerSolvedText,
            2 => twoCornersSolvedText,
            _ => allCornersSolvedText
        };

        SetObjectiveText(message);
    }

    private void Start()
    {
        StartCoroutine(ShowStartImageAfterDelay());

        if (bgmAudioSource != null)
        {
            bgmAudioSource.Play();
        }
    }

    // Wire this to the hotspot's open button.
    public void HideUIGroup()
    {
        if (uiGroup != null)
        {
            uiGroup.SetActive(false);
        }
    }

    // Wire this to the hotspot's close/exit button.
    public void ShowUIGroup()
    {
        if (uiGroup != null)
        {
            uiGroup.SetActive(true);
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

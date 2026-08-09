using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class JaydenDialogueController : MonoBehaviour
{
    private const float LockedAlpha = 180f / 255f;
    private const float UnlockedAlpha = 1f;

    [Header("Video Players")]
    [SerializeField] private VideoPlayer q1VideoPlayer;
    [SerializeField] private VideoPlayer q2VideoPlayer;
    [SerializeField] private VideoPlayer q3VideoPlayer;
    [SerializeField] private VideoPlayer riddleVideoPlayer;

    [Header("Buttons")]
    [SerializeField] private Button q1Button;
    [SerializeField] private Button q2Button;
    [SerializeField] private Button q3Button;
    [SerializeField] private Button riddleButton;

    [Header("Riddle Lock Visuals")]
    [SerializeField] private Image riddleBackgroundImage;
    [SerializeField] private TMP_Text riddleLabelText;

    [Header("Button Labels (Question -> Answer)")]
    [SerializeField] private TMP_Text question1Label;
    [SerializeField] private TMP_Text question2Label;
    [SerializeField] private TMP_Text question3Label;
    [SerializeField] private TMP_Text question4Label;
    [SerializeField] private TMP_Text answer1Label;
    [SerializeField] private TMP_Text answer2Label;
    [SerializeField] private TMP_Text answer3Label;
    [SerializeField] private TMP_Text answer4Label;
    [SerializeField] private bool answer1IsCorrect;
    [SerializeField] private bool answer2IsCorrect;
    [SerializeField] private bool answer3IsCorrect;
    [SerializeField] private bool answer4IsCorrect;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Riddle Result Events")]
    [SerializeField] private UnityEvent onCorrectAnswer;
    [SerializeField] private UnityEvent onWrongAnswer;

    private bool _q1Asked;
    private bool _q2Asked;
    private bool _q3Asked;
    private bool _isAnswerMode;

    private void Awake()
    {
        q1Button.onClick.AddListener(() => OnButtonClicked(1));
        q2Button.onClick.AddListener(() => OnButtonClicked(2));
        q3Button.onClick.AddListener(() => OnButtonClicked(3));
        riddleButton.onClick.AddListener(() => OnButtonClicked(4));

        riddleVideoPlayer.loopPointReached += OnRiddleVideoFinished;

        SetLabelAlpha(answer1Label, 0f);
        SetLabelAlpha(answer2Label, 0f);
        SetLabelAlpha(answer3Label, 0f);
        SetLabelAlpha(answer4Label, 0f);

        UpdateRiddleUnlockState();
    }

    private void OnButtonClicked(int index)
    {
        if (_isAnswerMode)
        {
            CheckAnswer(index);
        }
        else
        {
            AskQuestion(index);
        }
    }

    private void AskQuestion(int index)
    {
        switch (index)
        {
            case 1:
                PlayAnswer(q1VideoPlayer, 1);
                break;
            case 2:
                PlayAnswer(q2VideoPlayer, 2);
                break;
            case 3:
                PlayAnswer(q3VideoPlayer, 3);
                break;
            case 4:
                PlayAnswer(riddleVideoPlayer, 4);
                break;
        }
    }

    private void CheckAnswer(int index)
    {
        bool isCorrect = index switch
        {
            1 => answer1IsCorrect,
            2 => answer2IsCorrect,
            3 => answer3IsCorrect,
            4 => answer4IsCorrect,
            _ => false
        };

        if (isCorrect)
        {
            onCorrectAnswer.Invoke();
        }
        else
        {
            onWrongAnswer.Invoke();
        }
    }

    private void OnRiddleVideoFinished(VideoPlayer source)
    {
        _isAnswerMode = true;

        StartCoroutine(CrossfadeLabels(question1Label, answer1Label));
        StartCoroutine(CrossfadeLabels(question2Label, answer2Label));
        StartCoroutine(CrossfadeLabels(question3Label, answer3Label));
        StartCoroutine(CrossfadeLabels(question4Label, answer4Label));
    }

    private IEnumerator CrossfadeLabels(TMP_Text fadeOutLabel, TMP_Text fadeInLabel)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetLabelAlpha(fadeOutLabel, 1f - t);
            SetLabelAlpha(fadeInLabel, t);
            yield return null;
        }

        SetLabelAlpha(fadeOutLabel, 0f);
        SetLabelAlpha(fadeInLabel, 1f);
    }

    private void SetLabelAlpha(TMP_Text label, float alpha)
    {
        Color color = label.color;
        color.a = alpha;
        label.color = color;
    }

    private void PlayAnswer(VideoPlayer player, int questionIndex)
    {
        StopOtherVideoPlayers(player);
        player.Play();

        switch (questionIndex)
        {
            case 1:
                _q1Asked = true;
                break;
            case 2:
                _q2Asked = true;
                break;
            case 3:
                _q3Asked = true;
                break;
        }

        UpdateRiddleUnlockState();
    }

    private void StopOtherVideoPlayers(VideoPlayer except)
    {
        if (q1VideoPlayer != except && q1VideoPlayer.isPlaying)
        {
            q1VideoPlayer.Stop();
        }

        if (q2VideoPlayer != except && q2VideoPlayer.isPlaying)
        {
            q2VideoPlayer.Stop();
        }

        if (q3VideoPlayer != except && q3VideoPlayer.isPlaying)
        {
            q3VideoPlayer.Stop();
        }

        if (riddleVideoPlayer != except && riddleVideoPlayer.isPlaying)
        {
            riddleVideoPlayer.Stop();
        }
    }

    private void UpdateRiddleUnlockState()
    {
        bool allQuestionsAsked = _q1Asked && _q2Asked && _q3Asked;

        riddleButton.interactable = allQuestionsAsked;
        SetRiddleAlpha(allQuestionsAsked ? UnlockedAlpha : LockedAlpha);
    }

    private void SetRiddleAlpha(float alpha)
    {
        if (riddleBackgroundImage != null)
        {
            Color color = riddleBackgroundImage.color;
            color.a = alpha;
            riddleBackgroundImage.color = color;
        }

        if (riddleLabelText != null)
        {
            Color color = riddleLabelText.color;
            color.a = alpha;
            riddleLabelText.color = color;
        }
    }
}

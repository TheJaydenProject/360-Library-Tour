using UnityEngine;
using System.Collections;

public class RiddleOptions : MonoBehaviour
{
    [SerializeField] private GameObject rightAnswerImage;
    [SerializeField] private GameObject wrongAnswerImage;

    [SerializeField] private AudioSource correctAnswerAudioSource;
    [SerializeField] private AudioClip correctAnswerSfxClip;
    [SerializeField] private AudioSource wrongAnswerAudioSource;
    [SerializeField] private AudioClip wrongAnswerSfxClip;

    public void RightAnswer()
    {
        StopAllCoroutines();
        wrongAnswerImage.SetActive(false);
        rightAnswerImage.SetActive(false);
        rightAnswerImage.SetActive(true);
        StartCoroutine(WaitAndDeactivate(rightAnswerImage, 2f));
        PlaySfx(correctAnswerAudioSource, correctAnswerSfxClip);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ReportCornerSolved(UIManager.CornerType.Comic);
        }
    }

    public void WrongAnswer()
    {
        StopAllCoroutines();
        rightAnswerImage.SetActive(false);
        wrongAnswerImage.SetActive(false);
        wrongAnswerImage.SetActive(true);
        StartCoroutine(WaitAndDeactivate(wrongAnswerImage, 2f));
        PlaySfx(wrongAnswerAudioSource, wrongAnswerSfxClip);
    }

    private void PlaySfx(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null)
        {
            return;
        }

        source.PlayOneShot(clip);
    }

    private IEnumerator WaitAndDeactivate(GameObject image, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        image.SetActive(false);
    }
}

using UnityEngine;
using System.Collections;

public class RiddleOptions : MonoBehaviour
{
    [SerializeField] private GameObject rightAnswerImage;
    [SerializeField] private GameObject wrongAnswerImage;
    public void RightAnswer()
    {
        StopAllCoroutines();
        wrongAnswerImage.SetActive(false);
        rightAnswerImage.SetActive(true);
        StartCoroutine(WaitAndDeactivate(rightAnswerImage, 2f));
    }

    public void WrongAnswer()
    {
        StopAllCoroutines();
        rightAnswerImage.SetActive(false);
        wrongAnswerImage.SetActive(true);
        StartCoroutine(WaitAndDeactivate(wrongAnswerImage, 2f));
    }

    private IEnumerator WaitAndDeactivate(GameObject image, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        image.SetActive(false);
    }
}

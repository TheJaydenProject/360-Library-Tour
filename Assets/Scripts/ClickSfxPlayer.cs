using UnityEngine;

public class ClickSfxPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSfxClip;
    [SerializeField] private float clickSfxDelay = 0.1f;

    [SerializeField] private AudioSource secondAudioSource;
    [SerializeField] private AudioClip secondClickSfxClip;
    [SerializeField] private float secondClickSfxDelay = 0.1f;

    private int _clickCount;

    // Wire this to a Button's OnClick(). Uses AudioSource.PlayDelayed instead of a coroutine
    // so it still fires even if this GameObject gets deactivated later in the same click.
    public void PlayClickSfx()
    {
        if (clickSfxClip == null || audioSource == null)
        {
            return;
        }

        _clickCount++;

        audioSource.clip = clickSfxClip;
        audioSource.PlayDelayed(clickSfxDelay);

        bool isEvenClick = _clickCount % 2 == 0;
        if (isEvenClick && secondClickSfxClip != null && secondAudioSource != null)
        {
            secondAudioSource.clip = secondClickSfxClip;
            secondAudioSource.PlayDelayed(clickSfxDelay + clickSfxClip.length + secondClickSfxDelay);
        }
    }
}

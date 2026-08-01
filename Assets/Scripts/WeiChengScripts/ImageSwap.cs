using UnityEngine;
using UnityEngine.UI;

public class ImageSwap : MonoBehaviour
{
    Image sourceImage;
    Sprite originalSprite;
    bool isSwapped = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sourceImage = GetComponent<Image>();
        originalSprite = sourceImage.sprite;
    }

    public void SwapImage(Sprite newSprite)
    {
        if (sourceImage != null)
        {
            if (!isSwapped)
            {
                sourceImage.sprite = newSprite;
                isSwapped = true;
            }
            else
            {
                sourceImage.sprite = originalSprite;
                isSwapped = false;
            }
        }
    }
}

using UnityEngine;

public class RadialZoomBlurController : MonoBehaviour
{
    [SerializeField] private Material blurMaterial;

    private static readonly int BlurStrengthId = Shader.PropertyToID("_BlurStrength");

    public void SetBlurStrength(float strength)
    {
        if (blurMaterial == null)
        {
            Debug.LogError($"{nameof(RadialZoomBlurController)} has no material assigned.");
            return;
        }

        blurMaterial.SetFloat(BlurStrengthId, strength);
    }
}
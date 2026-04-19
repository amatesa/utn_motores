using UnityEngine;
using TMPro;

public class GlitchTextEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [Header("Glitch Settings")]
    [SerializeField] private float glitchInterval = 0.1f;
    [SerializeField] private float glitchIntensity = 2f;

    private Vector3 originalPosition;
    private Color originalColor;

    private void Start()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        originalPosition = text.rectTransform.localPosition;
        originalColor = text.color;

        InvokeRepeating(nameof(Glitch), glitchInterval, glitchInterval);
    }

    private void Glitch()
    {
        // Mover ligeramente
        float offsetX = Random.Range(-glitchIntensity, glitchIntensity);
        float offsetY = Random.Range(-glitchIntensity, glitchIntensity);

        text.rectTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

        // Flicker alpha
        float alpha = Random.Range(0.7f, 1f);
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        // Reset rápido
        CancelInvoke(nameof(ResetText));
        Invoke(nameof(ResetText), 0.05f);
    }

    private void ResetText()
    {
        text.rectTransform.localPosition = originalPosition;
        text.color = originalColor;
    }
}

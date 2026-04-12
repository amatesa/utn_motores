using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    [SerializeField] private Slider noiseSlider;

    void Update()
    {
        if (NoiseSystem.Instance == null)
        {
            Debug.LogWarning("[NoiseUI] NoiseSystem Instance is NULL");
            return;
        }

        if (noiseSlider == null)
        {
            Debug.LogWarning("[NoiseUI] Slider not assigned");
            return;
        }

        noiseSlider.value = NoiseSystem.Instance.GetNoise();
    }
}

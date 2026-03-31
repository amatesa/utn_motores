using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    public Slider noiseSlider;

    void Update()
    {
        if (NoiseSystem.Instance != null)
        {
            noiseSlider.value = NoiseSystem.Instance.currentNoise;
        }
    }
}

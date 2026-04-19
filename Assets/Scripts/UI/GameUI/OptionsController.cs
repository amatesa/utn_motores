using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Image overlayImage;

    [Header("Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer; // NUEVO

    [Header("Overlay Settings")]
    [SerializeField] private float overlayAlpha = 0.6f;

    private void Start()
    {
        CloseOptions();

        fullscreenToggle.isOn = Screen.fullScreen;

        // Inicializar slider (opcional)
        volumeSlider.value = 1f;
    }

    // =========================
    // OPEN / CLOSE
    // =========================
    public void OpenOptions()
    {
        optionsPanel.SetActive(true);

        overlayImage.gameObject.SetActive(true);

        Color c = overlayImage.color;
        c.a = overlayAlpha;
        overlayImage.color = c;
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        overlayImage.gameObject.SetActive(false);
    }

    // =========================
    // VOLUMEN REAL (dB)
    // =========================
    public void OnVolumeChanged(float value)
    {
        float dB = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);
    }

    // =========================
    // FULLSCREEN
    // =========================
    public void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
    }
}

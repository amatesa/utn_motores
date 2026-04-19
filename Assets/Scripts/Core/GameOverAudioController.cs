using UnityEngine;

public class GameOverAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource whisperSource;

    [Header("Volume")]
    [SerializeField] private float maxVolume = 0.6f;
    [SerializeField] private float volumeSpeed = 0.2f;

    [Header("Pitch")]
    [SerializeField] private float maxPitch = 1.3f;
    [SerializeField] private float pitchSpeed = 0.1f;

    [Header("Random Pitch")]
    [SerializeField] private float randomPitchMin = 0.98f;
    [SerializeField] private float randomPitchMax = 1.02f;
    [SerializeField] private float randomSpeed = 5f; // qué tan rápido cambia

    private float currentVolume = 0f;
    private float currentPitch = 1f;

    private void Start()
    {
        if (whisperSource == null)
            whisperSource = GetComponent<AudioSource>();

        whisperSource.volume = 0f;
        whisperSource.pitch = 1f;
    }

    private void Update()
    {
        // =========================
        // VOLUMEN PROGRESIVO
        // =========================
        currentVolume = Mathf.MoveTowards(currentVolume, maxVolume, volumeSpeed * Time.deltaTime);
        whisperSource.volume = currentVolume;

        // =========================
        // PITCH BASE PROGRESIVO
        // =========================
        currentPitch = Mathf.MoveTowards(currentPitch, maxPitch, pitchSpeed * Time.deltaTime);

        // =========================
        // RANDOM PITCH DINÁMICO
        // =========================
        float randomFactor = Mathf.Lerp(
            randomPitchMin,
            randomPitchMax,
            Mathf.PerlinNoise(Time.time * randomSpeed, 0f)
        );

        whisperSource.pitch = currentPitch * randomFactor;
    }
}

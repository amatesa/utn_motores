using StarterAssets;
using UnityEngine;

public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("Noise Settings (UI ONLY)")]
    [SerializeField] private float walkNoise = 5f;
    [SerializeField] private float runNoise = 12f;

    [Header("Event Noise (IA)")]
    [SerializeField] private float runEventIntensity = 8f; // 🔥 SOLO para eventos
    [SerializeField] private bool enableRunEvents = true;  // control fácil

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;

    private CharacterController controller;
    private StarterAssetsInputs input;

    [Header("Timing")]
    [SerializeField] private float runEventCooldown = 1.2f; // 🔥 menos frecuente

    [Header("Movement Threshold")]
    [SerializeField] private float minMovementSpeed = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private float lastRunEventTime;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        HandleNoise();
    }

    void HandleNoise()
    {
        // =========================
        // SAFE AREA
        // =========================
        if (PlayerSafeState.Instance.IsSafe)
        {
            StopAudio();
            return;
        }

        float speed = controller.velocity.magnitude;

        // =========================
        // NO MOVEMENT
        // =========================
        if (speed < minMovementSpeed)
        {
            StopAudio();
            return;
        }

        // =========================
        // STEALTH (NO SOUND AT ALL)
        // =========================
        if (input.stealth)
        {
            StopAudio();
            return;
        }

        // =========================
        // UI NOISE (SIEMPRE ACTIVO)
        // =========================
        if (input.sprint)
        {
            NoiseSystem.Instance.AddNoise(runNoise * Time.deltaTime);
            HandleAudio(runClip);

            HandleRunEvent(); // 🔥 SOLO sprint puede generar evento
        }
        else
        {
            NoiseSystem.Instance.AddNoise(walkNoise * Time.deltaTime);
            HandleAudio(walkClip);

            // NO EVENTOS EN WALK
        }
    }

    // =========================
    // EVENTOS CONTROLADOS
    // =========================
    void HandleRunEvent()
    {
        if (!enableRunEvents)
            return;

        if (Time.time - lastRunEventTime < runEventCooldown)
            return;

        lastRunEventTime = Time.time;

        EmitSound(runEventIntensity, "RUN_EVENT");
    }

    void EmitSound(float intensity, string type)
    {
        // pequeña desviación para evitar tracking perfecto
        Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
        randomOffset.y = 0;

        Vector3 soundPosition = transform.position + randomOffset;

        NoiseSystem.Instance.EmitSound(soundPosition, intensity);

        if (debugEnabled)
        {
            Debug.Log($"[PLAYER EVENT] {type} → intensity={intensity} pos={soundPosition}");
        }
    }

    // =========================
    // AUDIO
    // =========================
    void HandleAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}

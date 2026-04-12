using StarterAssets;
using UnityEngine;

public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("Noise Settings")]
    [SerializeField] private float walkNoise = 5f;
    [SerializeField] private float runNoise = 12f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;

    private CharacterController controller;
    private StarterAssetsInputs input;

    [Header("Sound Timing")]
    [SerializeField] private float walkCooldown = 0.6f;
    [SerializeField] private float runCooldown = 0.3f;

    [Header("Movement Threshold")]
    [SerializeField] private float minMovementSpeed = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private float lastSoundTime;

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
        // SAFE AREA
        if (PlayerSafeState.Instance.IsSafe)
        {
            StopAudio();
            return;
        }

        float speed = controller.velocity.magnitude;

        if (speed < minMovementSpeed)
        {
            StopAudio();
            return;
        }

        // STEALTH
        if (input.stealth)
        {
            StopAudio();
            return;
        }

        float currentCooldown = input.sprint ? runCooldown : walkCooldown;

        if (Time.time - lastSoundTime < currentCooldown)
            return;

        lastSoundTime = Time.time;

        if (input.sprint)
        {
            EmitSound(runNoise, "RUN");
            HandleAudio(runClip);
        }
        else
        {
            EmitSound(walkNoise, "WALK");
            HandleAudio(walkClip);
        }
    }

    void EmitSound(float intensity, string type)
    {
        // DESVIACIÓN → rompe tracking perfecto
        Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
        randomOffset.y = 0;

        Vector3 soundPosition = transform.position + randomOffset;

        NoiseSystem.Instance.EmitSound(soundPosition, intensity);

        if (debugEnabled)
        {
            Debug.Log($"[PLAYER SOUND] {type} → intensity={intensity} pos={soundPosition}");
        }
    }

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

using StarterAssets;
using UnityEngine;

/// <summary>
/// RESPONSABILIDAD:
/// - Generar ruido UI
/// - Generar eventos auditivos para IA
///
/// NOTA:
/// El audio de pasos fue separado a PlayerFootstepAudio
/// para respetar SRP (audio ≠ gameplay)
/// </summary>
public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("Noise Settings (UI ONLY)")]
    [SerializeField] private float walkNoise = 5f;
    [SerializeField] private float runNoise = 12f;

    [Header("Event Noise (IA)")]
    [SerializeField] private float runEventIntensity = 8f;
    [SerializeField] private bool enableRunEvents = true;

    private CharacterController controller;
    private StarterAssetsInputs input;

    [Header("Timing")]
    [SerializeField] private float runEventCooldown = 1.2f;

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
        ProcessNoise();
    }

    void ProcessNoise()
    {
        if (PlayerSafeState.Instance.IsSafe)
            return;

        float speed = controller.velocity.magnitude;

        if (speed < minMovementSpeed)
            return;

        if (input.stealth)
            return;

        if (input.sprint)
            HandleRun();
        else
            HandleWalk();
    }

    void HandleWalk()
    {
        // Solo feedback UI
        NoiseSystem.Instance.AddNoise(walkNoise * Time.deltaTime);
    }

    void HandleRun()
    {
        NoiseSystem.Instance.AddNoise(runNoise * Time.deltaTime);

        HandleRunEvent();
    }

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
        Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
        randomOffset.y = 0;

        Vector3 soundPosition = transform.position + randomOffset;

        NoiseSystem.Instance.EmitSound(
            soundPosition,
            intensity,
            SoundEmitterType.Player,
            gameObject
        );

        if (debugEnabled)
        {
            Debug.Log($"[PLAYER EVENT] {type} → intensity={intensity}");
        }
    }
}

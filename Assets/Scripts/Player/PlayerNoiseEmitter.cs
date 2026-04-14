using StarterAssets;
using UnityEngine;

/// <summary>
/// Convierte el movimiento del jugador en información

/// RESPONSABILIDAD:
/// - Generar ruido visual (UI)
/// - Generar eventos auditivos para la IA
/// - Reproducir audio de pasos (temporal)

/// INTERACCIONES:
/// - NoiseSystem → recibe ruido (UI + IA)
/// - PlayerSafeState → define si el jugador puede ser detectado
/// - Enemy (indirecto) → usa los eventos generados
///
/// DECISIÓN DE DISEÑO:
/// Este script centraliza todo el ruido del jugador para mantener coherencia.
/// En este momento mezcla responsabilidades (UI + IA + Audio), facilita testeo y control, pero
/// será refactorizado en el futuro para separar UI de IA si se vuelve demasiado complejo.
///
/// LÓGICA GENERAL:
/// 1- Validar si el jugador debe generar ruido (safe, quieto, stealth)
/// 2- Determinar tipo de movimiento (walk / run)
/// 3- Aplicar efectos correspondientes:
///     - UI noise (siempre)
///     - Audio (feedback)
///     - Eventos IA (solo en run)
/// </summary>
public class PlayerNoiseEmitter : MonoBehaviour
{
    // =========================
    // CONFIGURACIÓN DE RUIDO UI
    // =========================
    [Header("Noise Settings (UI ONLY)")]
    [SerializeField] private float walkNoise = 5f;
    [SerializeField] private float runNoise = 12f;

    // =========================
    // CONFIGURACIÓN EVENTOS IA
    // =========================
    [Header("Event Noise (IA)")]
    [SerializeField] private float runEventIntensity = 8f;
    [SerializeField] private bool enableRunEvents = true;

    // =========================
    // AUDIO (FEEDBACK JUGADOR)
    // =========================
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;

    // =========================
    // COMPONENTES
    // =========================
    private CharacterController controller;
    private StarterAssetsInputs input;

    // =========================
    // CONTROL DE EVENTOS
    // =========================
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

    // =========================
    // FLUJO PRINCIPAL
    // =========================
    void ProcessNoise()
    {
        // 1: Si está en zona segura, no debe generar ningún estímulo.
        // Esto evita que el enemigo tenga información residual del jugador.
        if (PlayerSafeState.Instance.IsSafe)
        {
            StopAudio();
            return;
        }
        // speed representa si el jugador se está moviendo o no, independientemente de la dirección. 
        float speed = controller.velocity.magnitude;

        // 2: Si no se mueve, no hay ruido.
        // Evita generar información falsa o innecesaria.
        if (speed < minMovementSpeed)
        {
            StopAudio();
            return;
        }

        // 3: Stealth cancela completamente el sistema.
        // Es una decisión de diseño: el jugador puede volverse "invisible" al sonido.
        // Solo anula input auditivo, no visual (Existe Raycast visual) permite estrategias de sigilo sin
        // ser completamente indetectable.
        if (input.stealth)
        {
            StopAudio();
            return;
        }

        // DECISIÓN DE ESTADO:
        // Se separa claramente caminar vs correr porque tienen impacto distinto en gameplay.
        if (input.sprint)
        {
            HandleRun();
        }
        else
        {
            HandleWalk();
        }
    }

    // =========================
    // WALK
    // =========================
    void HandleWalk()
    {
        // Ruido continuo para UI > feedback al jugador
        // No genera eventos IA para permitir movimiento "seguro"
        NoiseSystem.Instance.AddNoise(walkNoise * Time.deltaTime);

        // Audio de pasos
        PlayAudio(walkClip);
    }

    // =========================
    // RUN
    // =========================
    void HandleRun()
    {
        // Más ruido > mayor visibilidad para el jugador
        NoiseSystem.Instance.AddNoise(runNoise * Time.deltaTime);

        // Audio más intenso
        PlayAudio(runClip);

        // Genera eventos IA > esto es lo que realmente "activa" al enemigo
        HandleRunEvent();
    }

    // =========================
    // EVENTOS IA
    // =========================
    void HandleRunEvent()
    {
        // Permite desactivar fácilmente este comportamiento para testeo
        if (!enableRunEvents)
            return;

        // Cooldown evita spam > importante para:
        // - performance
        // - evitar comportamiento errático del enemigo
        if (Time.time - lastRunEventTime < runEventCooldown)
            return;

        lastRunEventTime = Time.time;

        EmitSound(runEventIntensity, "RUN_EVENT");
    }

    void EmitSound(float intensity, string type)
    {
        // Se agrega imprecisión intencional:
        // evita que el enemigo tenga tracking perfecto del jugador, insideUnitSphere genera un punto
        // aleatorio dentro de una esfera de radio 1, se escala a 0.5 para limitar el rango.
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
    void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        // Cambiar clip solo cuando es necesario evita cortes innecesarios
        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        // Si por algún motivo se detuvo, lo reanuda
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void StopAudio()
    {
        // Se detiene completamente para evitar sonidos residuales
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}

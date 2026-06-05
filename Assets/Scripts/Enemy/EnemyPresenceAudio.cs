using UnityEngine;

/// <summary>
/// Controla el audio de presencia del enemigo.
///
/// RESPONSABILIDAD:
/// Traducir el estado del enemigo y su distancia al jugador en capas de audio.
///
/// SISTEMA:
/// - Ambient > atmósfera constante (siempre activo)
/// - Whisper > anticipación (cerca pero sin amenaza directa)
/// - Attack > impacto (peligro real)
///
/// DISEÑO CLAVE:
/// Se usa un sistema híbrido:
/// - Loops (ambient + whisper) para continuidad
/// - Triggers (attack) para impacto
///
/// ¿POR QUÉ?:
/// - Solo loops > sensación plana
/// - Solo triggers > comportamiento caótico
/// - Combinación > tensión progresiva + momentos fuertes
///
/// DEPENDENCIAS:
/// - Usa distancia al jugador
/// - Usa estado real del enemigo (IsThreatActive)
///
/// IMPORTANTE:
/// Este script NO decide gameplay, solo lo representa en audio.
/// </summary>
public class EnemyPresenceAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private ShadowEnemyBrain enemy;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource whisperSource;
    [SerializeField] private AudioSource attackSource;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomPitch = false;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Whispers")]
    [SerializeField] private AudioClip[] whisperClips;
    [SerializeField] private float minWhisperDelay = 2f;
    [SerializeField] private float maxWhisperDelay = 5f;

    [Header("Distances")]
    [SerializeField] private float whisperDistance = 8f;
    [SerializeField] private float attackDistance = 3f;

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private AudioClip[] attackClips;

    private float lastAttackTime;
    private float nextWhisperTime;
    private int lastWhisperClipIndex = -1;
    private int lastAttackClipIndex = -1;
    private bool wasThreat;
    void Update()
    {
        // El audio depende del estado REAL del enemigo (FSM)
        // Esto asegura coherencia entre comportamiento y sonido
        bool isThreat = enemy != null && enemy.IsThreatActive();

        // Sin jugador no hay referencia espacial → no se procesa audio
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // DEBUG: útil para validar sistema en runtime
        //Debug.Log($"[AUDIO] dist={distance:F2} threat={isThreat}");

        // Sistema por capas (cada una independiente)
        HandleAmbient();
        HandleWhisper(distance, isThreat);
        HandleAttack(distance, isThreat);
    }


    // =========================
    // AMBIENT (BASE CONSTANTE)
    // =========================
    void HandleAmbient()
    {
        // Siempre tiende a un volumen base
        // Nunca se apaga → evita silencio total
        // Genera incomodidad constante
        // Math.Lerp para transición suave (evita cambios bruscos)
        float target = 0.6f;
        ambientSource.volume = Mathf.Lerp(
            ambientSource.volume,
            target,
            Time.deltaTime * fadeSpeed
        );
    }

    // =========================
    // WHISPER (ANTICIPACIÓN)
    // =========================
    void HandleWhisper(float distance, bool isThreat)
    {
        float target = 0f;

        // SOLO se activa si:
        // - el enemigo NO está en modo ataque
        // - el jugador está dentro del rango de proximidad
        //
        // Justificación:
        // Representa "presencia" sin confirmación visual
        if (!isThreat && distance < whisperDistance)
        {
            // Mapea distancia a volumen:
            // lejos → 0
            // cerca → 1
            target = Mathf.InverseLerp(whisperDistance, attackDistance, distance);
        }

        // Transición suave para evitar cambios bruscos
        whisperSource.volume = Mathf.Lerp(
            whisperSource.volume,
            target,
            Time.deltaTime * fadeSpeed
        );

        if (target > 0.01f)
            TryPlayWhisper();
    }

    // =========================
    // ATTACK (IMPACTO)
    // =========================
    void HandleAttack(float distance, bool isThreat)
    {
        // Dos formas de entrar en peligro:
        // 1. FSM del enemigo (chase)
        // 2. Proximidad extrema (fallback de seguridad)
        bool inRange = distance < attackDistance;
        bool shouldAttack = isThreat || inRange;

        // TRANSICIÓN:
        // Detecta el momento exacto en que entra en peligro
        //
        // Esto es clave para generar "impacto"
        if (shouldAttack && !wasThreat)
        {
            TryPlayAttack();
        }

        // ECO / PRESIÓN:
        // Si sigue en peligro, repite el sonido con intervalo
        //
        // Evita:
        // - spam constante
        // - silencio prolongado
        //
        // Genera ritmo (tensión intermitente)
        if (shouldAttack && Time.time - lastAttackTime > attackCooldown)
        {
            TryPlayAttack();
        }

        // Guarda estado anterior para detectar cambios
        wasThreat = shouldAttack;
    }

    void TryPlayAttack()
    {
        // Seguridad: evita errores si falta configuración
        if (attackSource == null) return;

        AudioClip clip = GetRandomClip(attackClips, attackSource.clip, ref lastAttackClipIndex);
        if (clip == null) return;

        lastAttackTime = Time.time;

        attackSource.Stop();
        PlayClip(attackSource, clip);
    }

    private void TryPlayWhisper()
    {
        if (whisperSource == null || Time.time < nextWhisperTime)
            return;

        AudioClip clip = GetRandomClip(whisperClips, whisperSource.clip, ref lastWhisperClipIndex);
        if (clip == null)
            return;

        whisperSource.loop = false;
        PlayClip(whisperSource, clip);

        float minDelay = Mathf.Max(0f, minWhisperDelay);
        float maxDelay = Mathf.Max(minDelay, maxWhisperDelay);
        nextWhisperTime = Time.time + Random.Range(minDelay, maxDelay);
    }

    private AudioClip GetRandomClip(AudioClip[] clips, AudioClip fallbackClip, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0)
            return fallbackClip;

        int index;
        if (clips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, clips.Length);
            }
            while (index == lastIndex);
        }

        lastIndex = index;
        return clips[index];
    }

    private void PlayClip(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null)
            return;

        float originalPitch = source.pitch;
        if (randomPitch)
            source.pitch = GetPlaybackPitch();

        source.PlayOneShot(clip, volume);
        source.pitch = originalPitch;
    }

    private float GetPlaybackPitch()
    {
        if (!randomPitch)
            return 1f;

        float min = Mathf.Min(pitchRange.x, pitchRange.y);
        float max = Mathf.Max(pitchRange.x, pitchRange.y);
        return Random.Range(min, max);
    }
}

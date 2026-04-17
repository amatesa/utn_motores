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

    [Header("Distances")]
    [SerializeField] private float whisperDistance = 8f;
    [SerializeField] private float attackDistance = 3f;

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;

    private float lastAttackTime;
    private bool wasThreat;

    void Update()
    {
        // Sin jugador no hay referencia espacial → no se procesa audio
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // El audio depende del estado REAL del enemigo (FSM)
        // Esto asegura coherencia entre comportamiento y sonido
        bool isThreat = enemy != null && enemy.IsThreatActive();

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
        ambientSource.volume = Mathf.Lerp(
            ambientSource.volume,
            0.6f,
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

        // FIX IMPORTANTE:
        // El volumen no reproduce audio por sí mismo.
        // Si el AudioSource no está en Play, no se escucha.
        //
        // Este check asegura que el loop se active cuando corresponde.
        if (target > 0.01f && !whisperSource.isPlaying)
        {
            whisperSource.Play();
        }
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
        if (attackSource == null || attackSource.clip == null) return;

        lastAttackTime = Time.time;

        // DECISIÓN CRÍTICA:
        // Se hace Stop + Play en lugar de solo Play
        //
        // Justificación:
        // - Reinicia el audio desde el inicio
        // - Garantiza impacto sonoro
        //
        // Sin esto:
        // el sonido puede no percibirse como nuevo evento
        attackSource.Stop();
        attackSource.Play();
    }
}

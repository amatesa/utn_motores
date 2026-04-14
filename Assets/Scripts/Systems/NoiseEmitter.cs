using UnityEngine;

/// <summary>
/// Genera eventos de sonido en el mundo.
/// RESPONSABILIDAD:
/// Emitir sonidos que el enemigo puede detectar.
///
/// INTERACCIONES:
/// - Envía a: NoiseSystem.EmitSound()
/// - Es consumido por: ShadowEnemy (hearing)
/// USO:
/// - Objetos del entorno (puertas, props, triggers)
/// - Eventos físicos (colisiones)
/// CONFIGURACIÓN:
/// - Puede emitir en Start, colisión o trigger
/// - Tiene cooldown para evitar spam
/// DISEÑO:
/// - Reutilizable (no depende del jugador)
/// - Genera eventos puntuales (no continuos)
/// </summary>
public class NoiseEmitter : MonoBehaviour
{
    [Header("Noise Settings")]
    [SerializeField] private float intensity = 10f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private bool useRandomOffset = true;

    [Header("Trigger Settings")]
    [SerializeField] private bool emitOnStart = false;
    [SerializeField] private bool emitOnCollision = false;
    [SerializeField] private bool emitOnTriggerEnter = false;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private float lastEmitTime;

    void Start()
    {
        // Emite sonido al iniciar si está habilitado
        if (emitOnStart)
        {
            Emit();
        }
    }

    /// <summary>
    /// Emite un evento de sonido manualmente.
    /// </summary>
    public void Emit()
    {
        // Controla cooldown para evitar spam
        if (Time.time - lastEmitTime < cooldown)
            return;

        lastEmitTime = Time.time;

        Vector3 pos = transform.position;

        // Agrega pequeña variación para evitar precisión perfecta
        if (useRandomOffset)
        {
            // Offset aleatorio en el plano horizontal
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = 0;
            pos += offset;
        }

        // Envía evento al sistema de sonido
        NoiseSystem.Instance.EmitSound(pos, intensity);

        if (debugEnabled)
        {
            Debug.Log($"[NoiseEmitter] EMIT → {gameObject.name} intensity={intensity}");
        }
    }

    // =========================
    // COLLISION
    // =========================

    void OnCollisionEnter(Collision collision)
    {
        // Emite sonido al colisionar si está habilitado
        if (!emitOnCollision) return;

        Emit();
    }

    // =========================
    // TRIGGER
    // =========================

    void OnTriggerEnter(Collider other)
    {
        // Emite sonido al entrar en trigger si está habilitado
        if (!emitOnTriggerEnter) return;

        // Solo si el jugador activa el trigger
        if (other.CompareTag("Player"))
        {
            Emit();
        }
    }
}

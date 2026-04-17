using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Sistema central de sonido del juego.
/// RESPONSABILIDAD:
/// Gestionar el ruido generado por el jugador y los eventos de sonido
/// que el enemigo puede percibir.
/// INTERACCIONES:
/// - Recibe de: PlayerNoiseEmitter, NoiseEmitter
/// - Es leído por: ShadowEnemy (hearing)
/// - Es leído por: NoiseUI (UI)
/// - Es modificado por: SafeArea (ClearSounds)

/// SISTEMAS INTERNOS:
/// 1. Noise (UI):
///     valor continuo para feedback visual (no afecta IA)
/// 2. SoundEvents (IA):
///     eventos discretos que el enemigo usa para investigar

/// DISEÑO ACTUAL:
/// - Ambos sistemas (UI + IA) están combinados en este script
/// - Rompe SRP pero simplifica la implementación actual

/// FUTURO (IMPORTANTE):
/// Este sistema debería separarse en:
///
/// - NoiseMeterSystem (UI)
///     manejo del ruido visual para el jugador
/// - SoundEventSystem (IA)
///     manejo de eventos de sonido para el enemigo
///
/// Separación esperada:
/// Player → SoundEventSystem → Enemy
/// Player → NoiseMeterSystem → UI

/// NOTA:
/// Validamos comportamiento primero, en etapa de refactor se separarán.
///
/// OTRAS MEJORAS FUTURAS:
/// - Diferentes tipos de sonido (pasos, objetos, etc.)
/// - Prioridades por tipo de evento
/// - Debug visual en escena
/// </summary>
public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance;

    // =========================
    // UI NOISE (NO IA)
    // =========================
    [Header("Legacy Noise (UI only)")]
    [SerializeField] private float currentNoise = 0f;
    public float maxNoise = 100f;
    public float decayRate = 5f;

    // =========================
    // SOUND EVENTS (IA)
    // =========================
    [Header("Sound Events")]
    [SerializeField] private float maxEventAge = 1.5f;
    [SerializeField] private float minDistanceBetweenEvents = 0.5f;
    [SerializeField] private float minEventIntensity = 2f;

    // Lista que almacena todos los eventos de sonido activos en el mundo
    private List<SoundEvent> soundEvents = new List<SoundEvent>();

    // =========================
    // DEBUG
    // =========================
    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private void Awake()
    {
        // Singleton
        if (Instance == null) 
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Reduce ruido visual con el tiempo
        DecayNoise();

        // Limpia eventos viejos
        CleanOldEvents();
    }

    // =========================
    // UI NOISE (NO IA)
    // =========================

    
    // Devuelve el nivel de ruido actual (solo UI)
    
    public float GetNoise()
    {
        return currentNoise;
    }

    /// <summary>
    // Aumenta el ruido (feedback visual)
    /// </summary>
    public void AddNoise(float amount)
    {
        currentNoise += amount;
        currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise); //Math.Clamp para evitar valores negativos o superiores al máximo

        if (debugEnabled)
            Debug.Log("[NoiseSystem] UI Noise = " + currentNoise);
    }

    /// <summary>
    /// Reduce el ruido con el tiempo
    /// </summary>
    private void DecayNoise()
    {
        if (currentNoise > 0)
        {
            currentNoise -= decayRate * Time.deltaTime;
            currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise); //Math.Clamp para evitar valores negativos
        }
    }

    // =========================
    // SOUND EVENTS (IA)
    // =========================

    /// <summary>
    /// Crea un evento de sonido en el mundo (usable por IA)
    /// El método se tiene que aplicar a eventos que el enemigo pueda percibir, no a cada ruido del jugador.
    /// </summary>
    public void EmitSound(Vector3 position, float intensity, SoundEmitterType emitter, GameObject source)
    {
        if (intensity < minEventIntensity)
        {
            if (debugEnabled)
                Debug.Log("[SoundEvent] Ignored (too weak)");
            return;
        }

        if (soundEvents.Count > 0)
        {
            SoundEvent last = soundEvents[soundEvents.Count - 1];

            float dist = Vector3.Distance(last.position, position);
            if (dist < minDistanceBetweenEvents)
            {
                if (debugEnabled)
                    Debug.Log("[SoundEvent] Ignored (too close)");
                return;
            }
        }

        SoundEvent soundEvent = new SoundEvent(position, intensity, emitter, source);

        soundEvents.Add(soundEvent);

        if (debugEnabled)
        {
            Debug.Log($"[SoundEvent] CREATED → {emitter} pos={position} intensity={intensity}");
        }

        bool isPlayerSource = source != null && source.CompareTag("Player");

        bool shouldAffectUI =
            emitter == SoundEmitterType.Player ||
            emitter == SoundEmitterType.Environment;

        // Excluir Enemy
        if (source != null && source.CompareTag("Enemy"))
        {
            shouldAffectUI = false;
        }

        if (shouldAffectUI)
        {
            AddNoise(intensity);
        }
        Debug.Log($"[NOISE_UI_TRIGGER] source={source.name} emitter={emitter}");
    }

    /// <summary>
    /// Devuelve una copia de los eventos actuales (fallback)
    /// </summary>
    public List<SoundEvent> GetSoundEvents()
    {
        return new List<SoundEvent>(soundEvents);
    }

    /// <summary>
    /// Limpia todos los eventos (ej: al entrar en SafeArea)
    /// </summary>
    public void ClearSounds()
    {
        // Limpia la lista de eventos
        soundEvents.Clear();

        if (debugEnabled)
        {
            Debug.Log("[SoundEvent] Cleared all events");
        }
    }

    /// <summary>
    /// Elimina eventos viejos automáticamente
    /// </summary>
    private void CleanOldEvents()
    {
        int before = soundEvents.Count;

        soundEvents.RemoveAll(e => Time.time - e.time > maxEventAge);

        int after = soundEvents.Count;

        if (before != after && debugEnabled)
        {
            Debug.Log($"[SoundEvent] Cleaned {before - after} events (remaining: {after})");
        }
    }

    // =========================
    // UTILIDAD IA
    // =========================

    /// <summary>
    /// Devuelve el evento más cercano dentro de un rango
    /// </summary>
    public SoundEvent? GetClosestSound(Vector3 listenerPosition, float maxRange)
    {
        //SoundEvent? es un tipo nullable que puede contener un SoundEvent o ser null,
        //lo que es útil para indicar que no se encontró ningún evento válido.

        SoundEvent? best = null;
        float bestDistance = Mathf.Infinity;

        foreach (var e in soundEvents)
        {
            float dist = Vector3.Distance(listenerPosition, e.position);

            if (dist > maxRange)
                continue;

            if (dist < bestDistance)
            {
                bestDistance = dist;
                best = e;
            }
        }

        return best;
    }
}

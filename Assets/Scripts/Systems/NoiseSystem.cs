using UnityEngine;
using System.Collections.Generic;

public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance;

    // =========================
    // LEGACY NOISE (UI ONLY)
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
    [SerializeField] private float minEventIntensity = 2f; // 🔥 NUEVO (filtro)

    private List<SoundEvent> soundEvents = new List<SoundEvent>();

    // =========================
    // DEBUG
    // =========================
    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private void Awake()
    {
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
        DecayNoise();
        CleanOldEvents();
    }

    // =========================
    // UI NOISE (NO IA)
    // =========================
    public float GetNoise()
    {
        return currentNoise;
    }

    public void AddNoise(float amount)
    {
        currentNoise += amount;
        currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise);

        if (debugEnabled)
            Debug.Log("[NoiseSystem] UI Noise = " + currentNoise);
    }

    private void DecayNoise()
    {
        if (currentNoise > 0)
        {
            currentNoise -= decayRate * Time.deltaTime;
            currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise);
        }
    }

    // =========================
    // SOUND EVENTS (IA)
    // =========================
    public void EmitSound(Vector3 position, float intensity)
    {
        //FILTRO DE INTENSIDAD (clave para diseño)
        if (intensity < minEventIntensity)
        {
            if (debugEnabled)
                Debug.Log("[SoundEvent] Ignored (too weak)");
            return;
        }

        //EVITAR EVENTOS MUY CERCANOS
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

        SoundEvent soundEvent = new SoundEvent(position, intensity);
        soundEvents.Add(soundEvent);

        if (debugEnabled)
        {
            Debug.Log($"[SoundEvent] CREATED → pos={position} intensity={intensity} total={soundEvents.Count}");
        }
    }

    //IMPORTANTE: copia defensiva
    public List<SoundEvent> GetSoundEvents()
    {
        return new List<SoundEvent>(soundEvents);
    }

    public void ClearSounds()
    {
        soundEvents.Clear();

        if (debugEnabled)
        {
            Debug.Log("[SoundEvent] Cleared all events");
        }
    }

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
    // EVENTO MÁS RELEVANTE
    // =========================
    public SoundEvent? GetClosestSound(Vector3 listenerPosition, float maxRange)
    {
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

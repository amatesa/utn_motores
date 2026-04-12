using UnityEngine;
using System.Collections.Generic;

public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance;

    [Header("Legacy Noise (UI only)")]
    [SerializeField] private float currentNoise = 0;
    public float maxNoise = 100f;
    public float decayRate = 5f;

    [Header("Sound Events")]
    [SerializeField] private float maxEventAge = 1.5f;
    [SerializeField] private float minDistanceBetweenEvents = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private List<SoundEvent> soundEvents = new List<SoundEvent>();

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

    void Update()
    {
        DecayNoise();
        CleanOldEvents();
    }

    // =========================
    // LEGACY (UI)
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
            Debug.Log("[NoiseSystem] CurrentNoise = " + currentNoise);
    }

    void DecayNoise()
    {
        if (currentNoise > 0)
        {
            currentNoise -= decayRate * Time.deltaTime;
            currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise);
        }
    }

    // =========================
    // SOUND EVENTS
    // =========================

    public void EmitSound(Vector3 position, float intensity)
    {
        // EVITAR EVENTOS DEMASIADO CERCANOS
        if (soundEvents.Count > 0)
        {
            SoundEvent last = soundEvents[soundEvents.Count - 1];

            float dist = Vector3.Distance(last.position, position);

            if (dist < minDistanceBetweenEvents)
            {
                return; // ignorar evento redundante
            }
        }

        SoundEvent soundEvent = new SoundEvent(position, intensity);
        soundEvents.Add(soundEvent);

        if (debugEnabled)
        {
            Debug.Log($"[SoundEvent] pos={position} intensity={intensity} total={soundEvents.Count}");
        }
    }

    // PROTEGIDO (no expone lista original)
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

    void CleanOldEvents()
    {
        int before = soundEvents.Count;

        soundEvents.RemoveAll(e => Time.time - e.time > maxEventAge);

        int after = soundEvents.Count;

        if (before != after && debugEnabled)
        {
            Debug.Log($"[SoundEvent] Cleaned {before - after} events (remaining: {after})");
        }
    }
}

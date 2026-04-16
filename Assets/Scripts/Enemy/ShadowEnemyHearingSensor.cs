using UnityEngine;

/// <summary>
/// Lectura de eventos sonoros para IA de enemigo.
/// </summary>
public class ShadowEnemyHearingSensor : MonoBehaviour
{
    [SerializeField] private bool debugEnabled = true;

    private float lastDebugSoundTime = -1f;

    public void ClearStaleContextOnStart()
    {
        if (NoiseSystem.Instance != null)
            NoiseSystem.Instance.ClearSounds();
    }

    public SoundEvent? GetRelevantSound(Vector3 listenerPosition, GameObject self)
    {
        if (NoiseSystem.Instance == null)
            return null;

        var events = NoiseSystem.Instance.GetSoundEvents();
        SoundEvent? latest = null;

        foreach (var e in events)
        {
            if (e.source == self)
                continue;

            float distance = Vector3.Distance(listenerPosition, e.position);
            float range = GetHearingRange(e.intensity);

            if (distance > range)
                continue;

            if (!latest.HasValue || e.time > latest.Value.time)
                latest = e;
        }

        if (latest.HasValue && latest.Value.time != lastDebugSoundTime)
        {
            lastDebugSoundTime = latest.Value.time;

            if (debugEnabled)
                Debug.Log("[ENEMY][HEARING] HEARD SOUND");
        }

        return latest;
    }

    private float GetHearingRange(float intensity)
    {
        return Mathf.Sqrt(intensity) * 10f;
    }
}

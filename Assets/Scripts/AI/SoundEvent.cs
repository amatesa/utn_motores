using UnityEngine;

public struct SoundEvent
{
    public Vector3 position;
    public float intensity;
    public float time;

    public SoundEvent(Vector3 pos, float intensity)
    {
        this.position = pos;
        this.intensity = intensity;
        this.time = Time.time;
    }
}

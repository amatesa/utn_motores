using UnityEngine;

public struct SoundEvent
{
    public Vector3 position;
    public float intensity;
    public float time;

    public SoundEmitterType emitter;
    public GameObject source;

    public SoundEvent(Vector3 pos, float intensity, SoundEmitterType emitter, GameObject source)
    {
        this.position = pos;
        this.intensity = intensity;
        this.time = Time.time;

        this.emitter = emitter;
        this.source = source;
    }
}

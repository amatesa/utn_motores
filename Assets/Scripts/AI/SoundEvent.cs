using UnityEngine;

/// Representa un sonido en el mundo.
/// Se usa para que el enemigo pueda "escuchar".
/// - Lo crea: NoiseSystem
/// - Lo usa: ShadowEnemy (hearing)
public struct SoundEvent
{
    public Vector3 position; // dónde ocurrió el sonido
    public float intensity;  // qué tan fuerte es (afecta rango)
    public float time;       // cuándo ocurrió

    public SoundEvent(Vector3 pos, float intensity)
    {
        this.position = pos;
        this.intensity = intensity;
        this.time = Time.time; // guarda el momento del evento
    }
}

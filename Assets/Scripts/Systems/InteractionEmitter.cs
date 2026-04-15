using UnityEngine;

public class InteractionEmitter : MonoBehaviour
{
    [SerializeField] private float noiseIntensity = 10f;
    [SerializeField] private SoundEmitterType emitterType;

    // 
    public void Interact()
    {
        EmitNoise(null);
    }

    // 
    public void Interact(GameObject instigator)
    {
        EmitNoise(instigator);
    }

    private void EmitNoise(GameObject instigator)
    {
        NoiseSystem.Instance.EmitSound(
            transform.position,
            noiseIntensity,
            emitterType,
            instigator != null ? instigator : gameObject
        );
    }
}

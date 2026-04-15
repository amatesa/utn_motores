using UnityEngine;

public class InteractionEmitter : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float noiseIntensity = 10f;

    [Header("Debug")]
    [SerializeField] private bool debug = true;

    public void Interact()
    {
        EmitNoise();
    }

    private void EmitNoise()
    {
        if (NoiseSystem.Instance == null)
        {
            Debug.LogWarning("[INTERACTION] NoiseSystem missing");
            return;
        }

        NoiseSystem.Instance.EmitSound(
             transform.position,
             noiseIntensity,
            SoundEmitterType.Player,
            gameObject
         );

        if (debug)
            Debug.Log($"[INTERACTION] Noise emitted at {transform.position} | intensity={noiseIntensity}");
    }
}

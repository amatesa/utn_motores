using UnityEngine;

/// <summary>
/// Feedback visual "shadow life" del enemigo.
/// </summary>
public class ShadowEnemyVisualController : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform[] shadowLayers;

    [Header("Shadow Life")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.15f;
    [SerializeField] private float deformSpeed = 1.3f;
    [SerializeField] private float deformAmount = 0.1f;
    [SerializeField] private float randomRotationAmount = 30f;
    [SerializeField] private float randomOffsetAmount = 0.3f;

    private float[] layerSeeds;

    public void Initialize()
    {
        if (shadowLayers == null)
            shadowLayers = new Transform[0];

        layerSeeds = new float[shadowLayers.Length];
        for (int i = 0; i < shadowLayers.Length; i++)
        {
            layerSeeds[i] = Random.Range(0f, 100f);
        }
    }

    public void Tick()
    {
        if (shadowLayers == null || layerSeeds == null)
            return;

        float time = Time.time;

        for (int i = 0; i < shadowLayers.Length; i++)
        {
            Transform layer = shadowLayers[i];
            if (layer == null)
                continue;

            float t = time + layerSeeds[i];

            float scaleX = 1 + Mathf.Sin(t * pulseSpeed) * pulseAmount;
            float scaleY = 1 + Mathf.Cos(t * deformSpeed) * deformAmount;
            float scaleZ = 1 + Mathf.Sin(t * deformSpeed * 0.7f) * deformAmount;

            layer.localScale = new Vector3(scaleX, scaleY, scaleZ);

            Vector3 offset = new Vector3(
                Mathf.Sin(t * 1.3f),
                0f,
                Mathf.Cos(t * 1.1f)
            ) * randomOffsetAmount;

            layer.localPosition = offset;

            float rot = Mathf.Sin(t * 0.5f) * randomRotationAmount;
            layer.localRotation = Quaternion.Euler(0f, rot, 0f);
        }
    }
}

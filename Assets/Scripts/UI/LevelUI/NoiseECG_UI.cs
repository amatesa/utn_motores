using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class NoiseECG_UI : MonoBehaviour
{
    [Header("Resolution")]
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 128;

    [Header("Wave Settings")]
    [SerializeField] private float baseLine = 0.5f;
    [SerializeField] private float decaySpeed = 2f;

    [Header("Spike Settings")]
    [SerializeField] private float spikeStrength = 30f;
    [SerializeField] private float spikeWidth = 0.05f;

    [Header("Limits")]
    [SerializeField] private float maxDisplacement = 40f;
    [SerializeField] private float maxGlitch = 15f;

    [Header("Enemy Influence")]
    [SerializeField] private Transform enemy;
    [SerializeField] private Transform player;
    [SerializeField] private float maxGlitchDistance = 10f;

    private Texture2D texture;
    private float[] buffer;
    private float currentNoise;

    void Start()
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        GetComponent<Image>().sprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f)
        );

        buffer = new float[width];
    }

    void Update()
    {
        if (NoiseSystem.Instance == null)
            return;

        float noise = NoiseSystem.Instance.GetNoise();

        UpdateBuffer(noise);
        Draw();
    }

    void UpdateBuffer(float noise)
    {
        currentNoise = Mathf.Lerp(currentNoise, noise, Time.deltaTime * decaySpeed);

        for (int i = 0; i < buffer.Length - 1; i++)
        {
            buffer[i] = buffer[i + 1];
        }

        float spike = GenerateSpike(currentNoise);

       
        spike = Mathf.Clamp(spike, -maxDisplacement, maxDisplacement);

        buffer[buffer.Length - 1] = spike;
    }

    float GenerateSpike(float noise)
    {
        if (noise < 0.1f)
            return 0;

        float spike = Mathf.Sin(Time.time * 20f) * spikeWidth;

        return spike * noise * spikeStrength;
    }

    float GetEnemyIntensity()
    {
        if (enemy == null || player == null)
            return 0f;

        float dist = Vector3.Distance(enemy.position, player.position);

        float t = Mathf.InverseLerp(maxGlitchDistance, 0f, dist);

        return Mathf.Clamp01(t);
    }

    void Draw()
    {
        float enemyIntensity = GetEnemyIntensity();

        // Clear
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }

        for (int x = 0; x < width; x++)
        {
            float baseY = height * baseLine;

            float displacement = buffer[x];

            // CLAMP BASE (extra seguridad)
            displacement = Mathf.Clamp(displacement, -maxDisplacement, maxDisplacement);

            // GLITCH CONTROLADO
            float glitch = (Random.value - 0.5f) * enemyIntensity * maxGlitch;
            glitch = Mathf.Clamp(glitch, -maxGlitch, maxGlitch);

            float y = baseY + displacement + glitch;

            int yInt = Mathf.Clamp((int)y, 0, height - 1);

            // COLOR DINÁMICO
            Color color = Color.Lerp(Color.green, Color.red, enemyIntensity);

            // GROSOR (mejora visual)
            for (int i = -1; i <= 1; i++)
            {
                int yy = Mathf.Clamp(yInt + i, 0, height - 1);
                texture.SetPixel(x, yy, color);
            }
        }

        texture.Apply();
    }
}

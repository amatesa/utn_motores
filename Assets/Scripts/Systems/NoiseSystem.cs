using UnityEngine;

public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance;

    public float currentNoise = 0f;

    public float maxNoise = 100f;
    public float decayRate = 5f;

    private void Awake()
    {
        Instance = this;
    }

    public void AddNoise(float amount)
    {
        currentNoise += amount;
        currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise);

        Debug.Log("Noise: " + currentNoise);
    }

    public void DecayNoise()
    {
        if (currentNoise > 0)
        {
            currentNoise -= decayRate * Time.deltaTime;
            currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise);

            Debug.Log("DecayNoise: " + currentNoise);
        }
    }
}

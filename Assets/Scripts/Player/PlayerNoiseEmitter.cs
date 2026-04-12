using StarterAssets;
using UnityEngine;

public class PlayerNoiseEmitter : MonoBehaviour
{
    public float walkNoise = 5f;
    public float runNoise = 10f;
    public AudioSource audioSource;
    public AudioClip walkClip;
    public AudioClip runClip;

    private CharacterController controller;
    private StarterAssetsInputs input;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        HandleNoise();
    }

    void HandleNoise()
    {
        bool isMoving = controller.velocity.magnitude > 0.1f;

        if (isMoving)
        {
            if (input.sprint)
            {
                NoiseSystem.Instance.AddNoise(runNoise * Time.deltaTime);
                HandleAudio(runClip);
            }
            else
            {
                NoiseSystem.Instance.AddNoise(walkNoise * Time.deltaTime);
                HandleAudio(walkClip);
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
    void PlaySound(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    void HandleAudio(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }


}

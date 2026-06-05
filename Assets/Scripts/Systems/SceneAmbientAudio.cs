using UnityEngine;

public class SceneAmbientAudio : MonoBehaviour
{
    [Header("Ambient Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = ambientClip;
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
    }

    private void Start()
    {
        if (!playOnAwake || audioSource == null || ambientClip == null)
            return;

        audioSource.Play();
    }
}

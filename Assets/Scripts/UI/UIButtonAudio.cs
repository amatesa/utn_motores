using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float hoverVolume = 1f;
    [SerializeField] private float clickVolume = 1f;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomPitch = false;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayClip(hoverClip, hoverVolume);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClip(clickClip, clickVolume);
    }

    private void PlayClip(AudioClip clip, float clipVolume)
    {
        if (audioSource == null || clip == null)
            return;

        float originalPitch = audioSource.pitch;

        if (randomPitch)
        {
            float min = Mathf.Min(pitchRange.x, pitchRange.y);
            float max = Mathf.Max(pitchRange.x, pitchRange.y);
            audioSource.pitch = Random.Range(min, max);
        }

        audioSource.PlayOneShot(clip, volume * clipVolume);
        audioSource.pitch = originalPitch;
    }
}

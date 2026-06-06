using UnityEngine;
using UnityEngine.UI;

public class InspectableDocumentViewer : MonoBehaviour
{
    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private GameObject gameplayHUD;

    [SerializeField] private MonoBehaviour[] behavioursToDisable;

    [Header("UI")]
    [SerializeField] private Image documentImage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private float openVolume = 1f;
    [SerializeField] private float closeVolume = 1f;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomPitch = false;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    public bool IsOpen { get; private set; }

    public void Open(Sprite documentSprite)
    {
        ThoughtPopupSystem.Instance?.ClearImmediate();

        if (documentImage != null)
            documentImage.sprite = documentSprite;

        PlayClip(openClip, openVolume);
        IsOpen = true;

        if (canvasRoot != null)
            canvasRoot.SetActive(true);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(false);

        foreach (MonoBehaviour behaviour in behavioursToDisable)
        {
            if (behaviour != null)
                behaviour.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        ThoughtPopupSystem.Instance?.ClearImmediate();

        PlayClip(closeClip, closeVolume);
        IsOpen = false;

        if (canvasRoot != null)
            canvasRoot.SetActive(false);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(true);

        foreach (MonoBehaviour behaviour in behavioursToDisable)
        {
            if (behaviour != null)
                behaviour.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void PlayClip(AudioClip clip, float clipVolume)
    {
        if (audioSource == null || clip == null)
            return;

        float originalPitch = audioSource.pitch;
        if (randomPitch)
            audioSource.pitch = GetPlaybackPitch();

        audioSource.PlayOneShot(clip, volume * clipVolume);
        audioSource.pitch = originalPitch;
    }

    private float GetPlaybackPitch()
    {
        if (!randomPitch)
            return 1f;

        float min = Mathf.Min(pitchRange.x, pitchRange.y);
        float max = Mathf.Max(pitchRange.x, pitchRange.y);
        return Random.Range(min, max);
    }
}

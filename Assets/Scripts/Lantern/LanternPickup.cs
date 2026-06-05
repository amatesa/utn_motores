using UnityEngine;

public class LanternPickup : MonoBehaviour
{
    [SerializeField] private GameObject playerLantern;

    [SerializeField] private LanternController lanternController;

    [SerializeField] private float initialFuelSeconds = 60f;

    [SerializeField]
    private AudioClip pickupSound;

    [SerializeField] private float volume = 1f;

    [SerializeField] private bool randomPitch = false;

    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [SerializeField]
    private string pickupThought =
        "This might keep it away.";

    private bool collected;

    public void CollectLantern()
    {
        if (collected)
            return;

        collected = true;
        PlayClip(pickupSound);

        if (playerLantern != null)
            playerLantern.SetActive(true);


        if (lanternController != null)
            lanternController.ActivateLanternWithFuel(initialFuelSeconds);


        if (ThoughtPopupSystem.Instance != null)
            ThoughtPopupSystem.Instance.ShowThought(pickupThought);

        Destroy(gameObject);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null)
            return;

        if (!randomPitch)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
            return;
        }

        GameObject audioObject = new GameObject($"{name}_OneShotAudio");
        audioObject.transform.position = transform.position;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = GetPlaybackPitch();
        source.Play();

        Destroy(audioObject, clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)));
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

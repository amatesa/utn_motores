using UnityEngine;

[DisallowMultipleComponent]
public class CandlePickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private int candleAmount = 1;
    [SerializeField] private CandleInventory inventory;
    [SerializeField] private bool destroyAfterPickup = true;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomPitch = false;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Thought")]
    [SerializeField] private bool showPickupThought = true;

    [TextArea]
    [SerializeField] private string pickupThought = "Una vela... debería guardarla.";

    [SerializeField] private float thoughtDuration = 4f;

    [SerializeField] private int thoughtPriority = 1;

    [SerializeField] private bool canInterrupt = false;

    [SerializeField] private ThoughtType thoughtType = ThoughtType.System;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    private bool collected;

    public void Collect()
    {
        if (collected)
            return;

        CandleInventory targetInventory =
            inventory != null ? inventory : CandleInventory.Instance;

        if (targetInventory == null)
        {
            Debug.LogWarning($"[CandlePickup] No CandleInventory available for {name}.");
            return;
        }

        if (!targetInventory.TryAddCandles(candleAmount))
        {
            Log("El inventario está lleno.");
            return;
        }

        collected = true;
        PlayClip(pickupSound);

        if (showPickupThought && ThoughtPopupSystem.Instance != null)
        {
            ThoughtPopupSystem.Instance.ShowThought(
                pickupThought,
                thoughtDuration,
                thoughtPriority,
                canInterrupt,
                thoughtType
            );
        }

        Log($"Collected {candleAmount} candle(s).");

        if (destroyAfterPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[CandlePickup] {message}");
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

using UnityEngine;

/// <summary>
/// World candle pickup. Adds candle charges to CandleInventory and then disables
/// itself. Intended to be called from Interactable.onInteract.
/// </summary>
[DisallowMultipleComponent]
public class CandlePickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private int candleAmount = 1;
    [SerializeField] private CandleInventory inventory;
    [SerializeField] private bool destroyAfterPickup = true;

    [Header("Thought")]
    [SerializeField] private bool showPickupThought = false;
    [TextArea]
    [SerializeField] private string pickupThought = "A candle. I should save it.";

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    private bool collected;

    /// <summary>
    /// Collects this candle pickup. Intended for Interactable UnityEvent wiring.
    /// </summary>
    public void Collect()
    {
        if (collected)
            return;

        CandleInventory targetInventory = inventory != null ? inventory : CandleInventory.Instance;

        if (targetInventory == null)
        {
            Debug.LogWarning($"[CandlePickup] No CandleInventory available for {name}.");
            return;
        }

        if (!targetInventory.TryAddCandles(candleAmount))
        {
            Log("Pickup ignored because candle inventory is full.");
            return;
        }

        collected = true;

        if (showPickupThought && ThoughtPopupSystem.Instance != null)
            ThoughtPopupSystem.Instance.ShowThought(pickupThought);

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
}

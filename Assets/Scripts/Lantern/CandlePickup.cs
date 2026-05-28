using UnityEngine;

[DisallowMultipleComponent]
public class CandlePickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private int candleAmount = 1;
    [SerializeField] private CandleInventory inventory;
    [SerializeField] private bool destroyAfterPickup = true;

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
}

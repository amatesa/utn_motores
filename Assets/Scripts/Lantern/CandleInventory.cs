using System;
using UnityEngine;

/// <summary>
/// Runtime candle resource store used by the lantern.
/// This is intentionally separate from the key inventory because candles are
/// consumable survival resources, not progression collectibles.
/// </summary>
[DisallowMultipleComponent]
public class CandleInventory : MonoBehaviour
{
    public static CandleInventory Instance { get; private set; }

    /// <summary>
    /// Raised whenever the candle count changes. Parameter is the new count.
    /// </summary>
    public event Action<int> OnCandleCountChanged;

    [Header("Candles")]
    [SerializeField] private int currentCandles;
    [SerializeField] private int maxCandles = 5;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    public int CurrentCandles => currentCandles;
    public int MaxCandles => maxCandles;
    public bool HasCandles => currentCandles > 0;
    public int AvailableSpace => Mathf.Max(0, maxCandles - currentCandles);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentCandles = Mathf.Clamp(currentCandles, 0, maxCandles);
    }

    /// <summary>
    /// Adds candles up to the configured maximum.
    /// </summary>
    public void AddCandles(int amount)
    {
        TryAddCandles(amount);
    }

    /// <summary>
    /// Attempts to add candles. Returns true if at least one candle was accepted.
    /// </summary>
    public bool TryAddCandles(int amount)
    {
        if (amount <= 0 || AvailableSpace <= 0)
            return false;

        int oldValue = currentCandles;
        currentCandles = Mathf.Clamp(currentCandles + amount, 0, maxCandles);

        if (currentCandles != oldValue)
        {
            OnCandleCountChanged?.Invoke(currentCandles);
            Log($"Candles added. Count={currentCandles}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to consume candles. Returns false if there are not enough.
    /// </summary>
    public bool TryConsumeCandles(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentCandles < amount)
            return false;

        currentCandles -= amount;
        OnCandleCountChanged?.Invoke(currentCandles);
        Log($"Candles consumed. Count={currentCandles}");
        return true;
    }

    /// <summary>
    /// Returns true when the requested amount can be consumed.
    /// </summary>
    public bool CanConsume(int amount)
    {
        return amount <= 0 || currentCandles >= amount;
    }

    [ContextMenu("Debug/Add One Candle")]
    private void DebugAddOneCandle()
    {
        AddCandles(1);
    }

    [ContextMenu("Debug/Refill Candles")]
    public void RefillCandles()
    {
        int oldValue = currentCandles;
        currentCandles = maxCandles;

        if (currentCandles != oldValue)
            OnCandleCountChanged?.Invoke(currentCandles);

        Log($"Candles refilled. Count={currentCandles}");
    }

    [ContextMenu("Debug/Clear Candles")]
    private void DebugClearCandles()
    {
        currentCandles = 0;
        OnCandleCountChanged?.Invoke(currentCandles);
        Log("Candles cleared.");
    }

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[CandleInventory] {message}");
    }
}

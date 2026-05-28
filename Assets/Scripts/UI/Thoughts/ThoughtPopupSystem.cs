using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central runtime service for atmospheric horror thought popups.
/// Future gameplay systems should call this service instead of talking directly
/// to UI objects, keeping ghost, lantern, enemy, and narrative systems decoupled.
/// </summary>
[DisallowMultipleComponent]
public class ThoughtPopupSystem : MonoBehaviour
{
    public static ThoughtPopupSystem Instance { get; private set; }

    /// <summary>
    /// Raised when a thought is accepted into the popup pipeline.
    /// Useful for future analytics, UI effects, or optional audio hooks.
    /// </summary>
    public event Action<ThoughtMessageData> OnThoughtQueued;

    /// <summary>
    /// Raised when a thought starts displaying on screen.
    /// </summary>
    public event Action<ThoughtMessageData> OnThoughtDisplayed;

    [Header("References")]
    [SerializeField] private ThoughtPopupUI popupUI;

    [Header("Defaults")]
    [SerializeField] private float defaultDuration = 3f;
    [SerializeField] private float messageCooldown = 1.25f;
    [SerializeField] private int defaultPriority = 0;
    [SerializeField] private bool defaultCanInterrupt = false;

    [Header("Spam Protection")]
    [SerializeField] private int maxQueuedMessages = 5;
    [SerializeField] private float duplicateCooldown = 6f;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;
    [SerializeField] private bool enableKeyboardDebugTrigger = false;
    [SerializeField] private Key debugKey = Key.F8;
    [TextArea]
    [SerializeField] private string debugMessage = "Something is watching me.";

    private readonly Queue<ThoughtMessageData> queuedMessages = new();
    private readonly Dictionary<string, float> recentMessages = new();

    private ThoughtMessageData? activeMessage;
    private float nextMessageTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (enableKeyboardDebugTrigger &&
            Keyboard.current != null &&
            Keyboard.current[debugKey].wasPressedThisFrame)
        {
            ShowThought(debugMessage);
        }

        CleanRecentMessages();
        TryDisplayNextQueuedMessage();
    }

    /// <summary>
    /// Queues a thought popup using the default duration and priority.
    /// </summary>
    public void ShowThought(string message)
    {
        ShowThought(message, defaultDuration);
    }

    /// <summary>
    /// Queues a thought popup using a custom duration and default priority.
    /// </summary>
    public void ShowThought(string message, float duration)
    {
        ShowThought(message, duration, defaultPriority, defaultCanInterrupt);
    }

    /// <summary>
    /// Queues a thought popup with explicit priority and interruption behavior.
    /// Higher priority messages may interrupt lower priority active messages.
    /// </summary>
    public void ShowThought(string message, float duration, int priority, bool canInterrupt)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (IsDuplicateCoolingDown(message))
        {
            Log($"Ignored duplicate thought during cooldown: {message}");
            return;
        }

        ThoughtMessageData data = new ThoughtMessageData(
            message,
            Mathf.Max(0.1f, duration),
            priority,
            canInterrupt
        );

        recentMessages[message] = Time.unscaledTime;

        if (CanInterruptActiveMessage(data))
        {
            OnThoughtQueued?.Invoke(data);
            InterruptWith(data);
            return;
        }

        if (queuedMessages.Count >= maxQueuedMessages)
        {
            Log($"Ignored thought because queue is full: {message}");
            return;
        }

        queuedMessages.Enqueue(data);
        OnThoughtQueued?.Invoke(data);
        Log($"Queued thought: {message}");
    }

    /// <summary>
    /// Clears queued thoughts and hides the active popup.
    /// Useful for future scene transitions or hard gameplay state changes.
    /// </summary>
    public void Clear()
    {
        queuedMessages.Clear();
        activeMessage = null;

        if (popupUI != null)
            popupUI.StopDisplay();
    }

    private void TryDisplayNextQueuedMessage()
    {
        if (popupUI == null || popupUI.IsDisplaying)
            return;

        if (queuedMessages.Count == 0)
            return;

        if (Time.unscaledTime < nextMessageTime)
            return;

        Display(queuedMessages.Dequeue());
    }

    private bool CanInterruptActiveMessage(ThoughtMessageData data)
    {
        if (!data.CanInterrupt || popupUI == null || !popupUI.IsDisplaying || !activeMessage.HasValue)
            return false;

        return data.Priority > activeMessage.Value.Priority;
    }

    private void InterruptWith(ThoughtMessageData data)
    {
        if (popupUI != null)
            popupUI.StopDisplay();

        Display(data);
    }

    private void Display(ThoughtMessageData data)
    {
        if (popupUI == null)
        {
            Debug.LogWarning("[ThoughtPopupSystem] Missing ThoughtPopupUI reference.");
            return;
        }

        activeMessage = data;
        nextMessageTime = Time.unscaledTime + data.Duration + messageCooldown;

        popupUI.Show(data.Message, data.Duration, () =>
        {
            activeMessage = null;
        });

        OnThoughtDisplayed?.Invoke(data);
        Log($"Displaying thought: {data.Message}");
    }

    private bool IsDuplicateCoolingDown(string message)
    {
        return recentMessages.TryGetValue(message, out float lastTime) &&
               Time.unscaledTime - lastTime < duplicateCooldown;
    }

    private void CleanRecentMessages()
    {
        if (recentMessages.Count == 0)
            return;

        List<string> expired = null;

        foreach (var pair in recentMessages)
        {
            if (Time.unscaledTime - pair.Value >= duplicateCooldown)
            {
                expired ??= new List<string>();
                expired.Add(pair.Key);
            }
        }

        if (expired == null)
            return;

        foreach (string key in expired)
        {
            recentMessages.Remove(key);
        }
    }

    [ContextMenu("Test Thought/Footsteps Upstairs")]
    private void TestFootstepsThought()
    {
        ShowThought("I hear footsteps upstairs...");
    }

    [ContextMenu("Test Thought/Watching")]
    private void TestWatchingThought()
    {
        ShowThought("Something is watching me.", defaultDuration, 1, true);
    }

    [ContextMenu("Test Thought/No Candles")]
    private void TestNoCandlesThought()
    {
        ShowThought("I don't have any candles left.", defaultDuration, 2, true);
    }

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[ThoughtPopupSystem] {message}");
    }
}

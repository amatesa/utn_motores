using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class ThoughtPopupSystem : MonoBehaviour
{
    public static ThoughtPopupSystem Instance { get; private set; }

    public event Action<ThoughtMessageData> OnThoughtQueued;
    public event Action<ThoughtMessageData> OnThoughtDisplayed;

    [Header("References")]
    [SerializeField] private ThoughtPopupUI popupUI;

    [Header("Defaults")]
    [SerializeField] private float defaultDuration = 3f;
    [SerializeField] private float messageCooldown = 1.25f;
    [SerializeField] private int defaultPriority = 0;
    [SerializeField] private bool defaultCanInterrupt = false;
    [SerializeField] private ThoughtType defaultThoughtType = ThoughtType.Flavor;

    [Header("Spam Protection")]
    [SerializeField] private int maxQueuedMessages = 5;
    [SerializeField] private float duplicateCooldown = 6f;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;
    [SerializeField] private bool enableKeyboardDebugTrigger = false;
    [SerializeField] private Key debugKey = Key.F8;

    [TextArea]
    [SerializeField]
    private string debugMessage = "Siento que algo me observa...";

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

    public void ShowThought(string message)
    {
        ShowThought(
            message,
            defaultDuration,
            defaultPriority,
            defaultCanInterrupt,
            defaultThoughtType
        );
    }

    public void ShowThought(string message, float duration)
    {
        ShowThought(
            message,
            duration,
            defaultPriority,
            defaultCanInterrupt,
            defaultThoughtType
        );
    }

    public void ShowThought(
        string message,
        float duration,
        int priority,
        bool canInterrupt,
        ThoughtType type
    )
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
            canInterrupt,
            type
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
        if (!data.CanInterrupt ||
            popupUI == null ||
            !popupUI.IsDisplaying ||
            !activeMessage.HasValue)
        {
            return false;
        }

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

        nextMessageTime =
            Time.unscaledTime +
            data.Duration +
            messageCooldown;

        popupUI.Show(
            data.Message,
            data.Duration,
            data.Type,
            () =>
            {
                activeMessage = null;
            }
        );

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

    [ContextMenu("Test Thought/Pasos")]
    private void TestFootstepsThought()
    {
        ShowThought(
            "Escucho pasos en el piso de arriba...",
            4f,
            0,
            false,
            ThoughtType.Flavor
        );
    }

    [ContextMenu("Test Thought/Peligro")]
    private void TestWatchingThought()
    {
        ShowThought(
            "Siento que algo me observa...",
            4f,
            5,
            true,
            ThoughtType.Danger
        );
    }

    [ContextMenu("Test Thought/Sin Velas")]
    private void TestNoCandlesThought()
    {
        ShowThought(
            "No me quedan velas...",
            4f,
            5,
            true,
            ThoughtType.Danger
        );
    }

    [ContextMenu("Test Thought/Objetivo")]
    private void TestObjectiveThought()
    {
        ShowThought(
            "Necesito encontrar una llave.",
            4f,
            2,
            false,
            ThoughtType.Objective
        );
    }

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[ThoughtPopupSystem] {message}");
    }
}

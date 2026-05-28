using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using StarterAssets;

/// <summary>
/// Dedicated opening book sequence shown after the intro cinematic and before
/// gameplay begins. This is separate from world readable documents.
/// </summary>
[DisallowMultipleComponent]
public class IntroBookSequence : MonoBehaviour
{
    /// <summary>
    /// Raised when the intro book opens.
    /// </summary>
    public event Action OnSequenceOpened;

    /// <summary>
    /// Raised when the intro book closes.
    /// </summary>
    public event Action OnSequenceClosed;

    [Header("References")]
    [SerializeField] private IntroBookUI bookUI;
    [SerializeField] private StarterAssetsInputs playerInputs;
    [SerializeField] private MonoBehaviour[] behavioursToDisableWhileOpen;
    [SerializeField] private GameObject[] objectsToHideWhileOpen;

    [Header("Pages")]
    [SerializeField] private List<IntroBookPageData> pages = new();

    [Header("Input")]
    [SerializeField] private InputActionReference closeAction;
    [SerializeField] private InputActionReference nextPageAction;
    [SerializeField] private InputActionReference previousPageAction;
    [SerializeField] private bool allowEscapeKeyClose = true;

    [Header("Cursor")]
    [SerializeField] private bool unlockCursorWhileOpen = true;
    [SerializeField] private bool restoreLockedCursorOnClose = true;

    [Header("Flow")]
    [SerializeField] private bool openOnStart = false;
    [SerializeField] private UnityEvent onSequenceClosed;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    private int currentPageIndex;
    private bool isOpen;
    private static bool hasAlreadyPlayed;

    /// <summary>
    /// True while the intro book is currently open.
    /// </summary>
    public bool IsOpen => isOpen;

    /// <summary>
    /// Current page index, zero-based.
    /// </summary>
    public int CurrentPageIndex => currentPageIndex;

    private void OnEnable()
    {
        EnableAction(closeAction);
        EnableAction(nextPageAction);
        EnableAction(previousPageAction);
    }

    private void Start()
    {
        if (openOnStart && !hasAlreadyPlayed)
        {
            hasAlreadyPlayed = true;
            OpenSequence();
        }
    }

    private void OnDisable()
    {
        DisableAction(closeAction);
        DisableAction(nextPageAction);
        DisableAction(previousPageAction);
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (WasTriggered(closeAction) ||
            (allowEscapeKeyClose && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            CloseSequence();
            return;
        }

        if (WasTriggered(nextPageAction))
            NextPage();

        if (WasTriggered(previousPageAction))
            PreviousPage();
    }

    /// <summary>
    /// Opens the intro book at the first page and blocks gameplay input.
    /// Intended to be called after the opening cinematic finishes.
    /// </summary>
    public void OpenSequence()
    {
        if (isOpen || bookUI == null || pages == null || pages.Count == 0)
            return;

        isOpen = true;
        currentPageIndex = 0;

        SetPlayerControlEnabled(false);
        SetCursorForSequence(true);

        RenderCurrentPage();
        bookUI.Show();

        OnSequenceOpened?.Invoke();
        Log("Intro book opened.");
    }

    /// <summary>
    /// Closes the intro book, restores gameplay input, and invokes the start-game hook.
    /// </summary>
    public void CloseSequence()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (bookUI != null)
            bookUI.Hide();

        gameObject.SetActive(false);

        SetPlayerControlEnabled(true);
        SetCursorForSequence(false);

        OnSequenceClosed?.Invoke();
        onSequenceClosed?.Invoke();

        Log("Intro book closed.");
    }

    /// <summary>
    /// Advances to the next page when available.
    /// </summary>
    public void NextPage()
    {
        if (!isOpen || pages == null || currentPageIndex >= pages.Count - 1)
            return;

        currentPageIndex++;
        RenderCurrentPage();
    }

    /// <summary>
    /// Returns to the previous page when available.
    /// </summary>
    public void PreviousPage()
    {
        if (!isOpen || currentPageIndex <= 0)
            return;

        currentPageIndex--;
        RenderCurrentPage();
    }

    private void RenderCurrentPage()
    {
        if (bookUI == null || pages == null || pages.Count == 0)
            return;

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pages.Count - 1);
        bookUI.Render(pages[currentPageIndex], currentPageIndex, pages.Count);
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (playerInputs != null)
        {
            playerInputs.enabled = enabled;

            if (!enabled)
            {
                playerInputs.move = Vector2.zero;
                playerInputs.look = Vector2.zero;
                playerInputs.jump = false;
                playerInputs.sprint = false;
                playerInputs.stealth = false;
                playerInputs.interact = false;
                playerInputs.switchCamera = false;
            }
        }

        if (behavioursToDisableWhileOpen == null)
            return;

        foreach (MonoBehaviour behaviour in behavioursToDisableWhileOpen)
        {
            if (behaviour != null)
                behaviour.enabled = enabled;
        }
        if (objectsToHideWhileOpen != null)
        {
            foreach (GameObject obj in objectsToHideWhileOpen)
            {
                if (obj != null)
                    obj.SetActive(enabled);
            }
        }
    }

    private void SetCursorForSequence(bool open)
    {
        if (!unlockCursorWhileOpen)
            return;

        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (restoreLockedCursorOnClose)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null)
            actionReference.action.Enable();
    }

    private static void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null)
            actionReference.action.Disable();
    }

    private static bool WasTriggered(InputActionReference actionReference)
    {
        return actionReference != null && actionReference.action.triggered;
    }

    [ContextMenu("Debug/Open Sequence")]
    private void DebugOpenSequence()
    {
        OpenSequence();
    }

    [ContextMenu("Debug/Close Sequence")]
    private void DebugCloseSequence()
    {
        CloseSequence();
    }

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[IntroBookSequence] {message}");
    }
}

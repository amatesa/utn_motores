using System;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;


[DisallowMultipleComponent]
public class ReadableDocumentViewer : MonoBehaviour
{
    public static ReadableDocumentViewer Instance { get; private set; }


    public event Action<ReadableDocumentData> OnDocumentOpened;

    public event Action<ReadableDocumentData> OnDocumentClosed;

    [Header("References")]
    [SerializeField] private ReadableDocumentUI documentUI;
    [SerializeField] private StarterAssetsInputs playerInputs;
    [SerializeField] private MonoBehaviour[] behavioursToDisableWhileReading;

    [Header("Input")]
    [SerializeField] private InputActionReference closeAction;
    [SerializeField] private InputActionReference nextPageAction;
    [SerializeField] private InputActionReference previousPageAction;
    [SerializeField] private bool allowEscapeKeyClose = true;

    [Header("Cursor")]
    [SerializeField] private bool unlockCursorWhileReading = true;
    [SerializeField] private bool restoreLockedCursorOnClose = true;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    private ReadableDocumentData currentDocument;
    private ReadableDocument currentWorldDocument;
    private int currentPageIndex;
    private bool isOpen;

    public bool IsOpen => isOpen;

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

    private void OnEnable()
    {
        EnableAction(closeAction);
        EnableAction(nextPageAction);
        EnableAction(previousPageAction);
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
            CloseDocument();
            return;
        }

        if (WasTriggered(nextPageAction))
            NextPage();

        if (WasTriggered(previousPageAction))
            PreviousPage();
    }


    public bool OpenDocument(ReadableDocumentData document, ReadableDocument worldDocument = null)
    {
        if (isOpen || document == null || documentUI == null)
            return false;

        currentDocument = document;
        currentWorldDocument = worldDocument;
        currentPageIndex = 0;
        isOpen = true;

        SetPlayerControlEnabled(false);
        SetCursorForReading(true);

        documentUI.Render(currentDocument, currentPageIndex);
        documentUI.Show();

        OnDocumentOpened?.Invoke(currentDocument);
        Log($"Opened document: {currentDocument.Title}");

        return true;
    }


    public void CloseDocument()
    {
        if (!isOpen)
            return;

        ReadableDocumentData closedDocument = currentDocument;
        ReadableDocument closedWorldDocument = currentWorldDocument;

        isOpen = false;
        currentDocument = null;
        currentWorldDocument = null;
        currentPageIndex = 0;

        if (documentUI != null)
            documentUI.Hide();

        SetPlayerControlEnabled(true);
        SetCursorForReading(false);

        closedWorldDocument?.NotifyClosed();
        TryTriggerThought(closedDocument);

        OnDocumentClosed?.Invoke(closedDocument);
        Log($"Closed document: {closedDocument?.Title}");
    }


    public void NextPage()
    {
        if (!isOpen || currentDocument == null)
            return;

        if (currentPageIndex >= currentDocument.PageCount - 1)
            return;

        currentPageIndex++;
        documentUI.Render(currentDocument, currentPageIndex);
    }


    public void PreviousPage()
    {
        if (!isOpen || currentDocument == null)
            return;

        if (currentPageIndex <= 0)
            return;

        currentPageIndex--;
        documentUI.Render(currentDocument, currentPageIndex);
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

        if (behavioursToDisableWhileReading == null)
            return;

        foreach (MonoBehaviour behaviour in behavioursToDisableWhileReading)
        {
            if (behaviour != null)
                behaviour.enabled = enabled;
        }
    }

    private void SetCursorForReading(bool reading)
    {
        if (!unlockCursorWhileReading)
            return;

        if (reading)
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

    private void TryTriggerThought(ReadableDocumentData document)
    {
        if (document == null || !document.ShowThoughtAfterReading)
            return;

        if (string.IsNullOrWhiteSpace(document.ThoughtMessage))
            return;

        if (ThoughtPopupSystem.Instance == null)
            return;

        ThoughtPopupSystem.Instance.ShowThought(
            document.ThoughtMessage,
            document.ThoughtDuration,
            document.ThoughtPriority,
            document.ThoughtCanInterrupt,
            ThoughtType.Objective
        );
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

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[ReadableDocumentViewer] {message}");
    }
}

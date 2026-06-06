using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Collections;

[DisallowMultipleComponent]
public class IntroBookSequence : MonoBehaviour
{

    public event Action OnSequenceOpened;


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

    [Header("Narration")]
    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioClip[] pageNarrations;
    [SerializeField] private float narrationVolume = 1f;

    [Header("Page Audio")]
    [SerializeField] private AudioSource pageAudioSource;
    [SerializeField] private AudioClip pageTurnClip;
    [SerializeField] private float pageTurnVolume = 1f;

    [Header("Narration Timing")]
    [SerializeField] private float firstPageNarrationDelay = 0.75f;
    [SerializeField] private float pageTurnNarrationDelay = 0.90f;

    private Coroutine narrationCoroutine;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    private int currentPageIndex;
    private bool isOpen;
    private static bool hasAlreadyPlayed;


    public bool IsOpen => isOpen;


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

        StartNarrationWithDelay(firstPageNarrationDelay);

        OnSequenceOpened?.Invoke();
        Log("Intro book opened.");
    }


    public void CloseSequence()
    {
        if (!isOpen)
            return;

        isOpen = false;

        StopNarration();

        if (bookUI != null)
            bookUI.Hide();

        gameObject.SetActive(false);

        SetPlayerControlEnabled(true);
        SetCursorForSequence(false);

        OnSequenceClosed?.Invoke();
        onSequenceClosed?.Invoke();

        Log("Intro book closed.");
    }


    public void NextPage()
    {
        if (!isOpen || pages == null || currentPageIndex >= pages.Count - 1)
            return;

        currentPageIndex++;

        RenderCurrentPage();

        PlayPageTurn();

        StartNarrationWithDelay(pageTurnNarrationDelay);
    }


    public void PreviousPage()
    {
        if (!isOpen || currentPageIndex <= 0)
            return;

        currentPageIndex--;

        RenderCurrentPage();

        PlayPageTurn();

        StartNarrationWithDelay(pageTurnNarrationDelay);
    }

    private void StartNarrationWithDelay(float delay)
    {
        if (narrationCoroutine != null)
            StopCoroutine(narrationCoroutine);

        narrationCoroutine = StartCoroutine(PlayNarrationDelayed(delay));
    }

    private IEnumerator PlayNarrationDelayed(float delay)
    {
        StopNarration();

        yield return new WaitForSeconds(delay);

        narrationCoroutine = null;

        PlayNarrationForCurrentPage();
    }

    private void RenderCurrentPage()
    {
        if (bookUI == null || pages == null || pages.Count == 0)
            return;

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pages.Count - 1);
        bookUI.Render(pages[currentPageIndex], currentPageIndex, pages.Count);
    }

    private void PlayPageTurn()
    {
        if (pageAudioSource == null || pageTurnClip == null)
            return;

        pageAudioSource.PlayOneShot(pageTurnClip, pageTurnVolume);
    }

    private void PlayNarrationForCurrentPage()
    {
        StopNarration();

        if (narrationSource == null || pageNarrations == null)
            return;

        if (currentPageIndex < 0 || currentPageIndex >= pageNarrations.Length)
            return;

        AudioClip narrationClip = pageNarrations[currentPageIndex];
        if (narrationClip == null)
            return;

        narrationSource.clip = narrationClip;
        narrationSource.volume = narrationVolume;
        narrationSource.Play();
    }

    private void StopNarration()
    {
        if (narrationCoroutine != null)
        {
            StopCoroutine(narrationCoroutine);
            narrationCoroutine = null;
        }

        if (narrationSource != null)
            narrationSource.Stop();
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

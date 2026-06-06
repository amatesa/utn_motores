using System;
using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class ThoughtPopupUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Transition")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Safety")]
    [SerializeField] private float maxDisplayDuration = 12f;

    private Coroutine displayRoutine;
    private Action activeOnComplete;
    private bool isDisplaying;
    private int displayVersion;

    public bool IsDisplaying => isDisplaying;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
    }

    private void OnDisable()
    {
        StopDisplay();
    }

    private void OnDestroy()
    {
        StopDisplay();
    }

    public Coroutine Show(
        string message,
        float duration,
        ThoughtType type,
        Action onComplete
    )
    {
        StopDisplay();

        isDisplaying = true;
        activeOnComplete = onComplete;
        int version = ++displayVersion;

        displayRoutine = StartCoroutine(
            ShowRoutine(message, duration, type, version)
        );

        return displayRoutine;
    }

    public void StopDisplay()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        activeOnComplete = null;
        isDisplaying = false;
        displayVersion++;
        HideImmediate();
    }

    private IEnumerator ShowRoutine(
        string message,
        float duration,
        ThoughtType type,
        int version
    )
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = GetThoughtColor(type);
        }

        SetVisibleState(true);

        float safeDuration = Mathf.Min(
            Mathf.Max(0.1f, duration),
            Mathf.Max(0.1f, maxDisplayDuration)
        );

        yield return Fade(0f, 1f, fadeInDuration);

        if (version != displayVersion)
            yield break;

        yield return new WaitForSecondsRealtime(safeDuration);

        if (version != displayVersion)
            yield break;

        yield return Fade(1f, 0f, fadeOutDuration);

        CompleteDisplay(version);
    }

    public void ForceHide()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        CompleteDisplay(displayVersion);
        displayVersion++;
    }

    private void CompleteDisplay(int version)
    {
        if (version != displayVersion)
            return;

        displayRoutine = null;
        isDisplaying = false;

        Action onComplete = activeOnComplete;
        activeOnComplete = null;

        HideImmediate();

        onComplete?.Invoke();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            canvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void SetVisibleState(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private Color GetThoughtColor(ThoughtType type)
    {
        switch (type)
        {
            case ThoughtType.Danger:
                return new Color(1f, 0.45f, 0.45f);

            case ThoughtType.Objective:
                return new Color(1f, 0.85f, 0.4f);

            case ThoughtType.System:
                return new Color(0.9f, 0.9f, 0.9f);

            default:
                return new Color(0.75f, 0.75f, 0.75f);
        }
    }
}

using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Canvas presentation component for atmospheric thought messages.
/// Owns only text visibility and fade animation; queueing and spam control live
/// in <see cref="ThoughtPopupSystem"/>.
/// </summary>
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

    private Coroutine displayRoutine;

    /// <summary>
    /// True while the UI is actively fading or showing a message.
    /// </summary>
    public bool IsDisplaying => displayRoutine != null;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
    }

    /// <summary>
    /// Displays a message with fade in, hold, and fade out.
    /// </summary>
    public Coroutine Show(string message, float duration, System.Action onComplete)
    {
        StopDisplay();
        displayRoutine = StartCoroutine(ShowRoutine(message, duration, onComplete));
        return displayRoutine;
    }

    /// <summary>
    /// Stops the current animation and hides the popup.
    /// </summary>
    public void StopDisplay()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        HideImmediate();
    }

    private IEnumerator ShowRoutine(string message, float duration, System.Action onComplete)
    {
        if (messageText != null)
            messageText.text = message;

        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSecondsRealtime(duration);
        yield return Fade(1f, 0f, fadeOutDuration);

        HideImmediate();
        displayRoutine = null;
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

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

    }
}

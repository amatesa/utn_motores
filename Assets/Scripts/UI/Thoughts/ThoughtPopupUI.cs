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

    private Coroutine displayRoutine;

    public bool IsDisplaying => displayRoutine != null;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
    }

    public Coroutine Show(
        string message,
        float duration,
        ThoughtType type,
        Action onComplete
    )
    {
        StopDisplay();

        displayRoutine = StartCoroutine(
            ShowRoutine(message, duration, type, onComplete)
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

        HideImmediate();
    }

    private IEnumerator ShowRoutine(
        string message,
        float duration,
        ThoughtType type,
        Action onComplete
    )
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = GetThoughtColor(type);
        }

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

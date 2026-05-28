using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas presenter for readable documents.
/// Owns only document UI rendering and transitions; gameplay opening/closing is
/// coordinated by <see cref="ReadableDocumentViewer"/>.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class ReadableDocumentUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI pageCounterText;

    [Header("Image")]
    [SerializeField] private Image pageImage;

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Transition")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    private Coroutine transitionRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
    }

    /// <summary>
    /// Updates text, image, page counter, and navigation affordances.
    /// </summary>
    public void Render(ReadableDocumentData document, int pageIndex)
    {
        if (document == null)
            return;

        int pageCount = document.PageCount;
        pageIndex = Mathf.Clamp(pageIndex, 0, pageCount - 1);

        if (titleText != null)
            titleText.text = document.Title;

        if (bodyText != null)
            bodyText.text = document.GetBodyText(pageIndex);

        if (pageCounterText != null)
            pageCounterText.text = pageCount > 1 ? $"{pageIndex + 1} / {pageCount}" : string.Empty;

        Sprite sprite = document.GetImage(pageIndex);
        if (pageImage != null)
        {
            pageImage.sprite = sprite;
            pageImage.enabled = sprite != null;
        }

        bool hasMultiplePages = pageCount > 1;

        if (previousButton != null)
        {
            previousButton.gameObject.SetActive(hasMultiplePages);
            previousButton.interactable = pageIndex > 0;
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(hasMultiplePages);
            nextButton.interactable = pageIndex < pageCount - 1;
        }
    }

    /// <summary>
    /// Fades the document UI into view.
    /// </summary>
    public void Show()
    {
        StartTransition(1f, fadeInDuration);
    }

    /// <summary>
    /// Fades the document UI out of view.
    /// </summary>
    public void Hide()
    {
        StartTransition(0f, fadeOutDuration);
    }

    private void StartTransition(float targetAlpha, float duration)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(FadeTo(targetAlpha, duration));
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        bool visible = canvasGroup.alpha > 0.01f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
        transitionRoutine = null;
    }

    private void HideImmediate()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}

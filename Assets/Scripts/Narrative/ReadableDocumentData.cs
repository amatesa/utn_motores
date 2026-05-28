using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authoring data for environmental narrative documents.
/// This asset intentionally contains presentation-neutral content so the same
/// document can be displayed by any future reader UI.
/// </summary>
[CreateAssetMenu(fileName = "ReadableDocument", menuName = "Silent Escape/Narrative/Readable Document")]
public class ReadableDocumentData : ScriptableObject
{
    [Header("Document")]
    [SerializeField] private string title;
    [TextArea(8, 24)]
    [SerializeField] private string bodyText;
    [SerializeField] private Sprite image;
    [SerializeField] private List<ReadableDocumentPage> pages = new();

    [Header("Thought Hook")]
    [SerializeField] private bool showThoughtAfterReading;
    [TextArea]
    [SerializeField] private string thoughtMessage;
    [SerializeField] private float thoughtDuration = 3f;
    [SerializeField] private int thoughtPriority = 0;
    [SerializeField] private bool thoughtCanInterrupt = false;

    [Header("Future Audio Hook")]
    [SerializeField] private bool requestSoundCue;
    [SerializeField] private string soundCueId;

    /// <summary>
    /// Display title for the document.
    /// </summary>
    public string Title => title;

    /// <summary>
    /// Fallback body text used when no explicit pages are configured.
    /// </summary>
    public string BodyText => bodyText;

    /// <summary>
    /// Fallback image used when no explicit pages are configured.
    /// </summary>
    public Sprite Image => image;

    /// <summary>
    /// Explicit pages. If empty, the document uses BodyText and Image as one page.
    /// </summary>
    public IReadOnlyList<ReadableDocumentPage> Pages => pages;

    public bool ShowThoughtAfterReading => showThoughtAfterReading;
    public string ThoughtMessage => thoughtMessage;
    public float ThoughtDuration => thoughtDuration;
    public int ThoughtPriority => thoughtPriority;
    public bool ThoughtCanInterrupt => thoughtCanInterrupt;

    /// <summary>
    /// Placeholder flag for future audio integration. No audio is played by this system.
    /// </summary>
    public bool RequestSoundCue => requestSoundCue;

    /// <summary>
    /// Placeholder cue identifier for future audio systems.
    /// </summary>
    public string SoundCueId => soundCueId;

    /// <summary>
    /// Returns the number of readable pages this document should expose.
    /// </summary>
    public int PageCount => pages != null && pages.Count > 0 ? pages.Count : 1;

    /// <summary>
    /// Returns body text for a page, falling back to the single body field.
    /// </summary>
    public string GetBodyText(int pageIndex)
    {
        if (pages != null && pages.Count > 0)
            return pages[Mathf.Clamp(pageIndex, 0, pages.Count - 1)].BodyText;

        return bodyText;
    }

    /// <summary>
    /// Returns an image for a page, falling back to the single image field.
    /// </summary>
    public Sprite GetImage(int pageIndex)
    {
        if (pages != null && pages.Count > 0)
            return pages[Mathf.Clamp(pageIndex, 0, pages.Count - 1)].Image;

        return image;
    }
}

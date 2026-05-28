using System;
using UnityEngine;

/// <summary>
/// One readable page inside a narrative document.
/// Documents may use a single page or several pages for letters, reports,
/// records, and longer orphanage lore.
/// </summary>
[Serializable]
public class ReadableDocumentPage
{
    [TextArea(6, 18)]
    [SerializeField] private string bodyText;
    [SerializeField] private Sprite image;

    /// <summary>
    /// Body text shown for this page.
    /// </summary>
    public string BodyText => bodyText;

    /// <summary>
    /// Optional image shown alongside this page.
    /// </summary>
    public Sprite Image => image;
}

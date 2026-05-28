using System;
using UnityEngine;

/// <summary>
/// One page in the dedicated intro narrative book sequence.
/// This is separate from readable world documents because the intro book owns
/// the game's opening pacing and start-game handoff.
/// </summary>
[Serializable]
public class IntroBookPageData
{
    [TextArea(6, 18)]
    [SerializeField] private string pageText;
    [SerializeField] private Sprite pageImage;

    /// <summary>
    /// Text shown on this intro book page.
    /// </summary>
    public string PageText => pageText;

    /// <summary>
    /// Optional image shown on this intro book page.
    /// </summary>
    public Sprite PageImage => pageImage;
}

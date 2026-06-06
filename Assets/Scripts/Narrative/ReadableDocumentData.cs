using System.Collections.Generic;
using UnityEngine;


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


    public string Title => title;


    public string BodyText => bodyText;

    public Sprite Image => image;

    public IReadOnlyList<ReadableDocumentPage> Pages => pages;

    public bool ShowThoughtAfterReading => showThoughtAfterReading;
    public string ThoughtMessage => thoughtMessage;
    public float ThoughtDuration => thoughtDuration;
    public int ThoughtPriority => thoughtPriority;
    public bool ThoughtCanInterrupt => thoughtCanInterrupt;


    public bool RequestSoundCue => requestSoundCue;

    public string SoundCueId => soundCueId;

    public int PageCount => pages != null && pages.Count > 0 ? pages.Count : 1;


    public string GetBodyText(int pageIndex)
    {
        if (pages != null && pages.Count > 0)
            return pages[Mathf.Clamp(pageIndex, 0, pages.Count - 1)].BodyText;

        return bodyText;
    }

 
    public Sprite GetImage(int pageIndex)
    {
        if (pages != null && pages.Count > 0)
            return pages[Mathf.Clamp(pageIndex, 0, pages.Count - 1)].Image;

        return image;
    }
}

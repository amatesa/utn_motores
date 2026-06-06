using System;
using UnityEngine;


[Serializable]
public class ReadableDocumentPage
{
    [TextArea(6, 18)]
    [SerializeField] private string bodyText;
    [SerializeField] private Sprite image;


    public string BodyText => bodyText;


    public Sprite Image => image;
}

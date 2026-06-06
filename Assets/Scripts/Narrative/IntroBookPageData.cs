using System;
using UnityEngine;


[Serializable]
public class IntroBookPageData
{
    [TextArea(6, 18)]
    [SerializeField] private string pageText;
    [SerializeField] private Sprite pageImage;


    public string PageText => pageText;


    public Sprite PageImage => pageImage;
}

using UnityEngine;

public class InspectableDocument : MonoBehaviour
{
    [SerializeField] private InspectableDocumentViewer viewer;
    [SerializeField] private Sprite documentSprite;

    public void Inspect()
    {
        if (viewer != null)
            viewer.Open(documentSprite);
    }
}

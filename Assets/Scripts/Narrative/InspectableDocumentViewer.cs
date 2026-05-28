using UnityEngine;
using UnityEngine.UI;

public class InspectableDocumentViewer : MonoBehaviour
{
    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private GameObject gameplayHUD;

    [SerializeField] private MonoBehaviour[] behavioursToDisable;

    [Header("UI")]
    [SerializeField] private Image documentImage;

    public void Open(Sprite documentSprite)
    {
        if (documentImage != null)
            documentImage.sprite = documentSprite;

        canvasRoot.SetActive(true);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(false);

        foreach (MonoBehaviour behaviour in behavioursToDisable)
        {
            if (behaviour != null)
                behaviour.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        canvasRoot.SetActive(false);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(true);

        foreach (MonoBehaviour behaviour in behavioursToDisable)
        {
            if (behaviour != null)
                behaviour.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

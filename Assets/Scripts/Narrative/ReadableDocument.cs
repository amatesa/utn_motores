using UnityEngine;


[DisallowMultipleComponent]
public class ReadableDocument : MonoBehaviour
{
    [Header("Document")]
    [SerializeField] private ReadableDocumentData documentData;
    [SerializeField] private ReadableDocumentViewer viewer;

    [Header("Interaction")]
    [SerializeField] private float reopenCooldown = 0.75f;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    private float nextAllowedOpenTime;


    public ReadableDocumentData DocumentData => documentData;


    public void OpenDocument()
    {
        if (Time.unscaledTime < nextAllowedOpenTime)
        {
            Log("Open ignored during cooldown.");
            return;
        }

        if (documentData == null)
        {
            Debug.LogWarning($"[ReadableDocument] Missing document data on {name}.");
            return;
        }

        ReadableDocumentViewer targetViewer = viewer != null ? viewer : ReadableDocumentViewer.Instance;

        if (targetViewer == null)
        {
            Debug.LogWarning($"[ReadableDocument] No ReadableDocumentViewer available for {name}.");
            return;
        }

        if (targetViewer.OpenDocument(documentData, this))
            Log($"Opened document: {documentData.Title}");
    }


    public void NotifyClosed()
    {
        nextAllowedOpenTime = Time.unscaledTime + reopenCooldown;
    }

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[ReadableDocument] {message}");
    }
}

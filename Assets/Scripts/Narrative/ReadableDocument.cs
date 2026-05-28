using UnityEngine;

/// <summary>
/// World component placed on a note, letter, record, or report.
/// The document remains in the scene and can be read repeatedly after a short
/// cooldown; it is not collected or added to inventory.
/// </summary>
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

    /// <summary>
    /// Document data assigned to this world object.
    /// </summary>
    public ReadableDocumentData DocumentData => documentData;

    /// <summary>
    /// Opens this document through the configured viewer.
    /// Intended to be called from an Interactable UnityEvent.
    /// </summary>
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

    /// <summary>
    /// Called by the viewer when this document closes.
    /// </summary>
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

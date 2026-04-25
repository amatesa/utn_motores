using UnityEngine;
/*
 Este sistema permite mostrar mensajes contextuales al jugador mediante triggers,
 mejorando el feedback sin acoplar directamente la lógica de gameplay con la UI.
 */
public class PopupTrigger : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string message;

    [SerializeField] private float duration = 3f;

    [SerializeField] private bool showOnlyOnce = true;
    // Evita mostrar múltiples veces el mismo mensaje
    private bool hasShown = false;

    private void OnTriggerEnter(Collider other)
    {
        // Solo el jugador activa el mensaje
        if (!other.CompareTag("Player")) return;

        if (showOnlyOnce && hasShown) return;

        PopupUIController popup = FindFirstObjectByType<PopupUIController>();

        if (popup != null)
        {
            popup.ShowMessage(message, duration);
        }

        hasShown = true;
    }
}

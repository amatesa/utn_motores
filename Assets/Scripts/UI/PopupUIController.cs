using TMPro;
using UnityEngine;
/*
 PopupUIController maneja la visualización de mensajes usando coroutines,
 permitiendo mostrar textos por un tiempo determinado sin bloquear la ejecución del juego.
 */
public class PopupUIController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;

    public void ShowMessage(string message, float duration)
    {
        /*
         CONTROL DE COROUTINES:
         Detiene cualquier mensaje anterior antes de mostrar uno nuevo.
         Evita solapamientos de mensajes.
        */
        StopAllCoroutines();
        /*
         INICIO DE RUTINA:
         Maneja la duración del mensaje en pantalla.
        */
        StartCoroutine(ShowRoutine(message, duration));
    }

    private System.Collections.IEnumerator ShowRoutine(string message, float duration)
    {
        panel.SetActive(true);
        messageText.text = message;

        yield return new WaitForSeconds(duration);

        panel.SetActive(false);
    }
}

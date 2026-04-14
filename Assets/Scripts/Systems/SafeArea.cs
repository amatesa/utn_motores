using UnityEngine;

/// Define una zona segura para el jugador.
/// RESPONSABILIDAD:
/// Detectar cuándo el jugador entra o sale de la zona
/// y actualizar su estado de seguridad.
/// INTERACCIONES:
/// - Escribe en: PlayerSafeState (IsSafe)
/// - Limpia: NoiseSystem (elimina sonidos activos)
/// EFECTO EN GAMEPLAY:
/// - El enemigo deja de perseguir al jugador
/// - Se eliminan estímulos de sonido (no puede investigar)
/// DISEÑO:
/// - Usa trigger collider
/// - No contiene lógica del enemigo, solo modifica estado global
public class SafeArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Detecta entrada del jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log("[SAFE AREA] ENTER");

            // Marca al jugador como seguro
            PlayerSafeState.Instance.SetSafe(true);

            // Limpia sonidos activos para evitar tracking del enemigo
            NoiseSystem.Instance.ClearSounds();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Detecta salida del jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log("[SAFE AREA] EXIT");

            // El jugador vuelve a ser detectable
            PlayerSafeState.Instance.SetSafe(false);
        }
    }
}

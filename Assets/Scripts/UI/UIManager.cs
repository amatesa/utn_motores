using UnityEngine;

/// Controlador de la interfaz de usuario (UI).
/// RESPONSABILIDAD:
/// Gestionar todos los elementos visuales del juego.
/// NO conteniene lógica de gameplay,
/// solo muestra información y responder a eventos.
///
/// FUTURO (lo que se espera agregar):
/// - Gestión de pantallas:
///     menú principal, pausa, game over
///
/// - UI dinámica:
///     barra de ruido, indicadores de peligro
///
/// - Feedback al jugador:
///     avisos visuales (detección, daño, tensión)
///
/// - Conexión con sistemas:
///     recibir datos de GameManager, NoiseSystem, Enemy
///
/// - Animaciones UI:
///     transiciones, fades, alertas
///
/// DISEÑO:
/// - Solo presentación (no lógica)
/// - Escucha eventos del juego
/// - Mantiene desacoplada la UI del gameplay
public class UIManager : MonoBehaviour
{
}

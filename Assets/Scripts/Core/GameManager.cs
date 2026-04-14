using UnityEngine;

/// Controlador principal del juego.
/// RESPONSABILIDAD:
/// Manejar el estado global del juego y coordinar sistemas.
/// NO debe contener lógica específica (enemigo, movimiento, audio),
/// solo orquestación de alto nivel.
/// FUTURO (lo que se espera agregar):
/// - Estados del juego:
///     (Inicio, Gameplay, Pausa, Game Over)
/// - Control de flujo:
///     iniciar partida, reiniciar, salir
/// - Referencias globales:
///     acceso a sistemas importantes (UI, player, enemigo)
/// - Eventos globales:
///     OnGameStart, OnGameOver, OnPause
///
/// - Integración con UI:
///     mostrar pantallas, feedback al jugador
/// - Control de dificultad:
///     escalar comportamiento del enemigo, a evaluar en un futuro.
///
/// DISEÑO:
/// - Delegar comportamiento a otros sistemas

public class GameManager : MonoBehaviour
{
}

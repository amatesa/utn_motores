using UnityEngine;

/// Maneja el estado de seguridad del jugador.
/// RESPONSABILIDAD:
/// Indicar si el jugador está en una zona segura.
/// INTERACCIONES:
/// - Es modificado por: SafeArea
/// - Es leído por: Enemy (ShadowEnemy), de momento hasta separar lógica por SRP
/// EFECTO EN GAMEPLAY:
/// - Si IsSafe = true → el enemigo no debe perseguir
/// - Si IsSafe = false → el jugador es detectable
/// DISEÑO:
/// - Singleton para acceso global
/// - Estado simple (bool)
/// - No contiene lógica compleja
/// FUTURO (posibles mejoras):
/// - Eventos: OnEnterSafe / OnExitSafe
/// - Feedback visual (UI)
/// - Cooldown al salir de zona segura

public class PlayerSafeState : MonoBehaviour
{
    public static PlayerSafeState Instance;

    [SerializeField] private bool isSafe = false;

    // Acceso público de solo lectura
    public bool IsSafe => isSafe;

    private void Awake()
    {
        // Singleton: asegura una sola instancia global
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    /// Cambia el estado de seguridad del jugador.
    public void SetSafe(bool value)
    {
        // Evita cambios innecesarios
        if (isSafe == value) return;

        isSafe = value;

        Debug.Log("[SAFE STATE] Player isSafe = " + isSafe);
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra el nivel de ruido del jugador en la UI.
/// RESPONSABILIDAD:
/// Leer el valor de ruido desde NoiseSystem
/// y actualizar un Slider en pantalla.
///
/// INTERACCIONES:
/// - Lee de: NoiseSystem.Instance.GetNoise()
/// - Muestra en: Slider (UI)
/// NO:
/// - No modifica el ruido
/// - No afecta comportamiento del enemigo
///
/// FUTURO (posibles mejoras):
/// - Suavizado visual (lerp del slider)
/// - Colores según nivel de peligro
/// - Efectos visuales (parpadeo, vibración)
/// </summary>
public class NoiseUI : MonoBehaviour
{
    [SerializeField] private Slider noiseSlider;

    void Update()
    {
        // Validación: NoiseSystem disponible
        if (NoiseSystem.Instance == null)
        {
            Debug.LogWarning("[NoiseUI] NoiseSystem Instance is NULL");
            return;
        }

        // Validación: Slider asignado
        if (noiseSlider == null)
        {
            Debug.LogWarning("[NoiseUI] Slider not assigned");
            return;
        }

        // Actualiza el valor del slider con el ruido actual
        noiseSlider.value = NoiseSystem.Instance.GetNoise();
    }
}

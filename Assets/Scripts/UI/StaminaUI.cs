using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private PlayerStaminaSystem staminaSystem;

    void Update()
    {
        if (staminaSystem == null || staminaSlider == null)
            return;

        staminaSlider.value = staminaSystem.CurrentStaminaNormalized;
    }
}

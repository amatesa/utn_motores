using UnityEngine;
using StarterAssets;

public class PlayerStaminaSystem : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float drainRate = 25f;
    [SerializeField] private float recoveryRate = 15f;
    [SerializeField] private float recoveryDelay = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = false;

    private float currentStamina;
    private float lastSprintTime;

    private StarterAssetsInputs input;

    public float CurrentStaminaNormalized => currentStamina / maxStamina;

    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        currentStamina = maxStamina;
    }

    void Update()
    {
        if (input == null) return;

        HandleStamina();
    }

    void HandleStamina()
    {
        bool isTryingToSprint = input.sprint;

        //BLOQUEO REAL
        if (currentStamina <= 0f)
        {
            input.sprint = false;
            isTryingToSprint = false;
        }

        if (isTryingToSprint)
        {
            DrainStamina();
        }
        else
        {
            RecoverStamina();
        }
    }

    void DrainStamina()
    {
        currentStamina -= drainRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        lastSprintTime = Time.time;
    }

    void RecoverStamina()
    {
        if (Time.time - lastSprintTime < recoveryDelay)
            return;

        currentStamina += recoveryRate * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
}

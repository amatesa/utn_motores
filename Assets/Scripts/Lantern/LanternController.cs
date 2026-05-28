using System;
using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
public class LanternController : MonoBehaviour
{

    public event Action<LanternState, LanternState> OnLanternStateChanged;

    public event Action<float, float> OnFuelChanged;

    [Header("References")]
    [SerializeField] private CandleInventory candleInventory;
    [SerializeField] private LanternProtectionZone protectionZone;
    [SerializeField] private Light[] lanternLights;
    [SerializeField] private GameObject[] activeVisuals;

    [Header("Input")]
    [SerializeField] private InputActionReference toggleAction;

    [Header("Fuel")]
    [SerializeField] private float fuelSecondsPerCandle = 20f;
    [SerializeField] private float baseDrainPerSecond = 1f;
    [SerializeField] private float enemyPressureDrainMultiplier = 0.45f;
    [SerializeField] private float fadingThoughtThreshold = 0.25f;

    [Header("Flicker")]
    [SerializeField] private bool enableLowFuelFlicker = true;
    [SerializeField] private float flickerThreshold = 0.2f;
    [SerializeField] private float minFlickerIntensity = 1.5f;
    [SerializeField] private float maxFlickerIntensity = 3f;
    [SerializeField] private float flickerSpeed = 18f;

    [Header("Cooldown")]
    [SerializeField] private float baseCooldownAfterOff = 3f;
    [SerializeField] private float dangerousPhaseCooldownMultiplier = 1.35f;

    [Header("Phase Scaling")]
    [SerializeField] private float explorationDrainMultiplier = 1f;
    [SerializeField] private float keyHuntDrainMultiplier = 1.2f;
    [SerializeField] private float escapeDrainMultiplier = 1.45f;
    [SerializeField] private float finalEscapeDrainMultiplier = 1.75f;
    [SerializeField] private float dangerProtectionEfficiency = 0.75f;
    [SerializeField] private float finalEscapeProtectionEfficiency = 0.55f;

    [Header("Thoughts")]
    [SerializeField] private bool showThoughts = true;

    [TextArea]
    [SerializeField] private string noCandlesThought = "No me quedan velas...";

    [TextArea]
    [SerializeField] private string fadingThought = "La llama se está apagando...";

    [TextArea]
    [SerializeField] private string unstableThought = "La llama no resistirá mucho más...";

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    [SerializeField] private LanternState currentState = LanternState.Off;
    [SerializeField] private float currentFuelSeconds;
    [SerializeField] private float cooldownRemaining;

    private bool fadingThoughtShown;
    private bool unstableThoughtShown;
    private float inputBlockTimer;
    public LanternState CurrentState => currentState;
    public bool IsActive => currentState == LanternState.On;
    public bool IsCoolingDown => currentState == LanternState.Cooldown;
    public float CurrentFuelSeconds => currentFuelSeconds;
    public float MaxFuelSeconds => fuelSecondsPerCandle;
    public float FuelNormalized => fuelSecondsPerCandle <= 0f ? 0f : Mathf.Clamp01(currentFuelSeconds / fuelSecondsPerCandle);
    public float CooldownRemaining => cooldownRemaining;
    public float CurrentDrainPerSecond => CalculateCurrentDrainPerSecond();
    public float CurrentProtectionEfficiency => CalculateProtectionEfficiency();

    private void OnEnable()
    {
        if (toggleAction != null)
            toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        if (toggleAction != null)
            toggleAction.action.Disable();
    }

    private void Start()
    {
        ApplyLanternVisuals(IsActive);

        if (protectionZone != null)
        {
            protectionZone.SetActive(IsActive);
            protectionZone.SetProtectionEfficiency(CurrentProtectionEfficiency);
        }
    }

    private void Update()
    {
        if (inputBlockTimer > 0f)
        {
            inputBlockTimer -= Time.deltaTime;
        }
        else if (toggleAction != null && toggleAction.action.triggered)
        {
            ToggleLantern();
        }

        TickCooldown();

        if (IsActive)
        {
            TickFuelDrain();
            TickFlicker();
        }
    }

    /// <summary>
    /// Toggles the lantern on or off.
    /// </summary>
    public void ToggleLantern()
    {
        if (IsActive)
        {
            TurnOff(true);
            return;
        }

        TryTurnOn();
    }

    /// <summary>
    /// Attempts to turn the lantern on, consuming a candle if new fuel is needed.
    /// </summary>
    public bool TryTurnOn()
    {
        if (currentState == LanternState.Cooldown)
        {
            Log("Lantern is cooling down.");
            return false;
        }

        if (currentFuelSeconds <= 0f && !TryLoadCandle())
        {
            ChangeState(LanternState.Empty);
            ShowThought(noCandlesThought, 2, true);
            return false;
        }

        fadingThoughtShown = false;
        unstableThoughtShown = false;

        ChangeState(LanternState.On);
        ApplyLanternVisuals(true);

        if (protectionZone != null)
        {
            protectionZone.SetProtectionEfficiency(CurrentProtectionEfficiency);
            protectionZone.SetActive(true);
        }

        OnFuelChanged?.Invoke(currentFuelSeconds, fuelSecondsPerCandle);
        Log("Lantern turned on.");
        return true;
    }
    /// <summary>
    /// Used by the first lantern pickup to immediately activate the lantern
    /// with initial fuel already loaded.
    /// </summary>
    public void ActivateLanternWithFuel(float fuelAmount)
    {
        currentFuelSeconds = fuelAmount;
        inputBlockTimer = 0.35f;
        fadingThoughtShown = false;
        unstableThoughtShown = false;

        ChangeState(LanternState.On);

        ApplyLanternVisuals(true);

        if (protectionZone != null)
        {
            protectionZone.SetProtectionEfficiency(CurrentProtectionEfficiency);
            protectionZone.SetActive(true);
        }

        OnFuelChanged?.Invoke(currentFuelSeconds, fuelSecondsPerCandle);

        Log("Lantern activated from pickup.");
    }
    /// <summary>
    /// Turns the lantern off and optionally starts cooldown.
    /// </summary>
    public void TurnOff(bool startCooldown)
    {
        if (!IsActive && currentState != LanternState.Empty)
            return;

        ApplyLanternVisuals(false);

        if (protectionZone != null)
            protectionZone.SetActive(false);

        if (startCooldown)
        {
            cooldownRemaining = CalculateCooldownDuration();
            ChangeState(cooldownRemaining > 0f ? LanternState.Cooldown : LanternState.Off);
        }
        else
        {
            ChangeState(currentFuelSeconds > 0f ? LanternState.Off : LanternState.Empty);
        }

        Log("Lantern turned off.");
    }

    /// <summary>
    /// Debug helper for draining fuel immediately.
    /// </summary>
    public void DrainFuel(float amount)
    {
        if (amount <= 0f)
            return;

        currentFuelSeconds = Mathf.Max(0f, currentFuelSeconds - amount);
        OnFuelChanged?.Invoke(currentFuelSeconds, fuelSecondsPerCandle);

        if (currentFuelSeconds <= 0f && IsActive)
            HandleFuelDepleted();
    }

    private void TickCooldown()
    {
        if (currentState != LanternState.Cooldown)
            return;

        cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);

        if (cooldownRemaining <= 0f)
            ChangeState(currentFuelSeconds > 0f ? LanternState.Off : LanternState.Empty);
    }

    private void TickFuelDrain()
    {
        float drain = CalculateCurrentDrainPerSecond() * Time.deltaTime;
        currentFuelSeconds = Mathf.Max(0f, currentFuelSeconds - drain);

        if (protectionZone != null)
            protectionZone.SetProtectionEfficiency(CurrentProtectionEfficiency);

        OnFuelChanged?.Invoke(currentFuelSeconds, fuelSecondsPerCandle);
        TryShowPressureThoughts();

        if (currentFuelSeconds <= 0f)
            HandleFuelDepleted();
    }

    private void TickFlicker()
    {
        if (!enableLowFuelFlicker)
            return;

        if (FuelNormalized > flickerThreshold)
        {
            ResetLightIntensity();
            return;
        }

        if (lanternLights == null)
            return;

        float flicker =
            Mathf.Lerp(
                minFlickerIntensity,
                maxFlickerIntensity,
                Mathf.PerlinNoise(Time.time * flickerSpeed, 0f)
            );

        foreach (Light lanternLight in lanternLights)
        {
            if (lanternLight != null)
                lanternLight.intensity = flicker;
        }
    }

    private void ResetLightIntensity()
    {
        if (lanternLights == null)
            return;

        foreach (Light lanternLight in lanternLights)
        {
            if (lanternLight != null)
                lanternLight.intensity = maxFlickerIntensity;
        }
    }
    private bool TryLoadCandle()
    {
        CandleInventory inventory = candleInventory != null ? candleInventory : CandleInventory.Instance;

        if (inventory == null || !inventory.TryConsumeCandles(1))
            return false;

        currentFuelSeconds = fuelSecondsPerCandle;
        OnFuelChanged?.Invoke(currentFuelSeconds, fuelSecondsPerCandle);
        return true;
    }

    private void HandleFuelDepleted()
    {
        currentFuelSeconds = 0f;
        TurnOff(false);
        ShowThought(fadingThought, 1, true);
    }

    private float CalculateCurrentDrainPerSecond()
    {
        float phaseMultiplier = GetPhaseDrainMultiplier();
        float pressureMultiplier = 1f;

        if (protectionZone != null && protectionZone.IsActive)
            pressureMultiplier += protectionZone.EnemyPressureCount * enemyPressureDrainMultiplier;

        return baseDrainPerSecond * phaseMultiplier * pressureMultiplier;
    }

    private float GetPhaseDrainMultiplier()
    {
        GamePhaseSystem phaseSystem = GamePhaseSystem.Instance;

        if (phaseSystem == null)
            return explorationDrainMultiplier;

        switch (phaseSystem.CurrentPhase)
        {
            case GamePhase.KeyHunt:
                return keyHuntDrainMultiplier;
            case GamePhase.Escape:
                return escapeDrainMultiplier;
            case GamePhase.FinalEscape:
                return finalEscapeDrainMultiplier;
            case GamePhase.Intro:
            case GamePhase.Exploration:
            default:
                return explorationDrainMultiplier;
        }
    }

    private float CalculateProtectionEfficiency()
    {
        GamePhaseSystem phaseSystem = GamePhaseSystem.Instance;

        if (phaseSystem == null)
            return 1f;

        switch (phaseSystem.CurrentPhase)
        {
            case GamePhase.FinalEscape:
                return finalEscapeProtectionEfficiency;
            case GamePhase.KeyHunt:
            case GamePhase.Escape:
                return dangerProtectionEfficiency;
            default:
                return 1f;
        }
    }

    private float CalculateCooldownDuration()
    {
        float cooldown = baseCooldownAfterOff;

        if (GamePhaseSystem.Instance != null && GamePhaseSystem.Instance.IsDangerPhase())
            cooldown *= dangerousPhaseCooldownMultiplier;

        return cooldown;
    }

    private void TryShowPressureThoughts()
    {
        if (fuelSecondsPerCandle <= 0f)
            return;

        if (!fadingThoughtShown && FuelNormalized <= fadingThoughtThreshold)
        {
            fadingThoughtShown = true;
            ShowThought(fadingThought, 0, false);
        }

        bool enemyPressure = protectionZone != null && protectionZone.EnemyPressureCount > 0;
        bool dangerousPhase = GamePhaseSystem.Instance != null && GamePhaseSystem.Instance.IsDangerPhase();

        if (!unstableThoughtShown && enemyPressure && dangerousPhase)
        {
            unstableThoughtShown = true;
            ShowThought(unstableThought, 1, false);
        }
    }

    private void ApplyLanternVisuals(bool active)
    {
        if (lanternLights != null)
        {
            foreach (Light lanternLight in lanternLights)
            {
                if (lanternLight != null)
                    lanternLight.enabled = active;
            }
        }

        if (activeVisuals != null)
        {
            foreach (GameObject visual in activeVisuals)
            {
                if (visual != null)
                    visual.SetActive(active);
            }
        }
    }

    private void ChangeState(LanternState newState)
    {
        if (currentState == newState)
            return;

        LanternState oldState = currentState;
        currentState = newState;
        OnLanternStateChanged?.Invoke(oldState, newState);
        Log($"State changed: {oldState} -> {newState}");
    }

    private void ShowThought(string message, int priority, bool canInterrupt)
    {
        if (!showThoughts ||
            string.IsNullOrWhiteSpace(message) ||
            ThoughtPopupSystem.Instance == null)
        {
            return;
        }

        ThoughtType type = ThoughtType.System;

        if (priority >= 5)
            type = ThoughtType.Danger;

        ThoughtPopupSystem.Instance.ShowThought(
            message,
            3f,
            priority,
            canInterrupt,
            type
        );
    }

    [ContextMenu("Debug/Toggle Lantern")]
    private void DebugToggleLantern()
    {
        ToggleLantern();
    }

    [ContextMenu("Debug/Load One Candle Fuel")]
    private void DebugLoadOneCandleFuel()
    {
        currentFuelSeconds = fuelSecondsPerCandle;
        OnFuelChanged?.Invoke(currentFuelSeconds, fuelSecondsPerCandle);
    }

    [ContextMenu("Debug/Drain 5 Seconds")]
    private void DebugDrainFiveSeconds()
    {
        DrainFuel(5f);
    }

    [ContextMenu("Debug/Empty Fuel")]
    private void DebugEmptyFuel()
    {
        DrainFuel(currentFuelSeconds);
    }

    private void Log(string message)
    {
        if (logDebugMessages)
            Debug.Log($"[LanternController] {message}");
    }
}

using System;
using UnityEngine;


[DisallowMultipleComponent]
[RequireComponent(typeof(ShadowEnemyBrain))]
[RequireComponent(typeof(ShadowEnemyMovementController))]
[RequireComponent(typeof(ShadowEnemyHearingSensor))]
[RequireComponent(typeof(ShadowEnemyTeleportController))]
public class EnemyAggressionController : MonoBehaviour
{

    public event Action<EnemyAggressionProfile> OnAggressionProfileChanged;

    [Header("Profiles")]
    [SerializeField] private EnemyAggressionProfile lowAggression;
    [SerializeField] private EnemyAggressionProfile moderateAggression;
    [SerializeField] private EnemyAggressionProfile highAggression;
    [SerializeField] private EnemyAggressionProfile extremeAggression;

    [Header("References")]
    [SerializeField] private ShadowEnemyBrain brain;
    [SerializeField] private ShadowEnemyMovementController movement;
    [SerializeField] private ShadowEnemyHearingSensor hearing;
    [SerializeField] private ShadowEnemyTeleportController teleport;

    [Header("Runtime")]
    [SerializeField] private EnemyAggressionLevel currentLevel = EnemyAggressionLevel.Low;
    [SerializeField] private EnemyAggressionProfile currentProfile;

    [Header("Debug")]
    [SerializeField] private bool applyProfileOnStart = true;
    [SerializeField] private bool logAggressionChanges = true;

    private bool subscribedToPhaseSystem;

    public EnemyAggressionLevel CurrentLevel => currentLevel;
    public EnemyAggressionProfile CurrentProfile => currentProfile;
    public float CurrentLanternResistance => currentProfile != null ? currentProfile.LanternResistance : 0f;
    public float CurrentRetreatResistance => currentProfile != null ? currentProfile.RetreatResistance : 0f;
    public float CurrentStalkingPressure => currentProfile != null ? currentProfile.StalkingPressure : 0f;
    public float EffectiveLanternProtectionMultiplier => 1f - CurrentLanternResistance;

    private void Awake()
    {
        if (brain == null)
            brain = GetComponent<ShadowEnemyBrain>();

        if (movement == null)
            movement = GetComponent<ShadowEnemyMovementController>();

        if (hearing == null)
            hearing = GetComponent<ShadowEnemyHearingSensor>();

        if (teleport == null)
            teleport = GetComponent<ShadowEnemyTeleportController>();
    }

    private void OnEnable()
    {
        TrySubscribeToPhaseSystem();
    }

    private void Start()
    {
        TrySubscribeToPhaseSystem();

        if (applyProfileOnStart)
            ApplyProfileForCurrentPhase();
    }

    private void OnDisable()
    {
        if (subscribedToPhaseSystem && GamePhaseSystem.Instance != null)
        {
            GamePhaseSystem.Instance.OnPhaseChanged -= HandlePhaseChanged;
            subscribedToPhaseSystem = false;
        }
    }

    public void ApplyProfile(EnemyAggressionProfile profile)
    {
        if (profile == null)
        {
            Debug.LogWarning($"[EnemyAggressionController] Missing aggression profile on {name}.");
            return;
        }

        currentProfile = profile;
        currentLevel = profile.AggressionLevel;

        movement?.ApplyAggressionSpeedMultiplier(profile.MovementSpeedMultiplier);
        hearing?.ApplyHearingSensitivity(profile.HearingSensitivity);
        teleport?.ApplyTeleportFrequency(profile.TeleportFrequency);
        brain?.ApplyAggressionTiming(
            profile.ChasePersistence,
            profile.SearchDuration,
            profile.HesitationDuration
        );

        OnAggressionProfileChanged?.Invoke(profile);

        if (logAggressionChanges)
            Debug.Log($"[EnemyAggressionController] Applied {profile.DisplayName} ({profile.AggressionLevel}) on {name}");
    }

 
    public void ForceAggressionLevel(EnemyAggressionLevel level)
    {
        ApplyProfile(GetProfileForLevel(level));
    }


    public float GetEffectiveLanternProtection(float baseProtection)
    {
        return Mathf.Clamp01(baseProtection * EffectiveLanternProtectionMultiplier);
    }


    public void ApplyProfileForCurrentPhase()
    {
        GamePhase phase = GamePhaseSystem.Instance != null
            ? GamePhaseSystem.Instance.CurrentPhase
            : GamePhase.Exploration;

        ApplyProfile(GetProfileForPhase(phase));
    }

    private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
    {
        ApplyProfile(GetProfileForPhase(newPhase));
    }

    private void TrySubscribeToPhaseSystem()
    {
        if (subscribedToPhaseSystem || GamePhaseSystem.Instance == null)
            return;

        GamePhaseSystem.Instance.OnPhaseChanged += HandlePhaseChanged;
        subscribedToPhaseSystem = true;
    }

    private EnemyAggressionProfile GetProfileForPhase(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.KeyHunt:
                return moderateAggression;
            case GamePhase.Escape:
                return highAggression;
            case GamePhase.FinalEscape:
                return extremeAggression;
            case GamePhase.Intro:
            case GamePhase.Exploration:
            default:
                return lowAggression;
        }
    }

    private EnemyAggressionProfile GetProfileForLevel(EnemyAggressionLevel level)
    {
        switch (level)
        {
            case EnemyAggressionLevel.Moderate:
                return moderateAggression;
            case EnemyAggressionLevel.High:
                return highAggression;
            case EnemyAggressionLevel.Extreme:
                return extremeAggression;
            case EnemyAggressionLevel.Low:
            default:
                return lowAggression;
        }
    }

    [ContextMenu("Force Aggression/Low")]
    private void DebugForceLow()
    {
        ForceAggressionLevel(EnemyAggressionLevel.Low);
    }

    [ContextMenu("Force Aggression/Moderate")]
    private void DebugForceModerate()
    {
        ForceAggressionLevel(EnemyAggressionLevel.Moderate);
    }

    [ContextMenu("Force Aggression/High")]
    private void DebugForceHigh()
    {
        ForceAggressionLevel(EnemyAggressionLevel.High);
    }

    [ContextMenu("Force Aggression/Extreme")]
    private void DebugForceExtreme()
    {
        ForceAggressionLevel(EnemyAggressionLevel.Extreme);
    }
}

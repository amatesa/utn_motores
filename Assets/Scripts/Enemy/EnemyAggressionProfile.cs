using UnityEngine;

/// <summary>
/// Data-driven tuning profile for enemy horror pressure.
/// Values are multipliers unless otherwise noted, allowing the existing enemy
/// modules to keep their local base tuning.
/// </summary>
[CreateAssetMenu(fileName = "EnemyAggressionProfile", menuName = "Silent Escape/Enemy/Aggression Profile")]
public class EnemyAggressionProfile : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private EnemyAggressionLevel aggressionLevel = EnemyAggressionLevel.Low;
    [SerializeField] private string displayName = "Low Aggression";

    [Header("Movement")]
    [Min(0.1f)]
    [SerializeField] private float movementSpeedMultiplier = 1f;

    [Header("Perception")]
    [Min(0.1f)]
    [SerializeField] private float hearingSensitivity = 1f;

    [Header("Chase")]
    [Min(0.1f)]
    [SerializeField] private float chasePersistence = 1f;

    [Header("Teleport")]
    [Min(0.1f)]
    [Tooltip("Higher values make teleport checks happen sooner.")]
    [SerializeField] private float teleportFrequency = 1f;

    [Header("Retreat")]
    [Range(0f, 1f)]
    [SerializeField] private float retreatResistance = 0f;
    [Min(0.1f)]
    [SerializeField] private float hesitationDuration = 1f;

    [Header("Lantern")]
    [Range(0f, 1f)]
    [SerializeField] private float lanternResistance = 0f;

    [Header("Search")]
    [Min(0.1f)]
    [SerializeField] private float searchDuration = 1f;

    [Header("Psychological Pressure")]
    [Range(0f, 1f)]
    [SerializeField] private float stalkingPressure = 0f;

    public EnemyAggressionLevel AggressionLevel => aggressionLevel;
    public string DisplayName => displayName;
    public float MovementSpeedMultiplier => movementSpeedMultiplier;
    public float HearingSensitivity => hearingSensitivity;
    public float ChasePersistence => chasePersistence;
    public float TeleportFrequency => teleportFrequency;
    public float RetreatResistance => retreatResistance;
    public float HesitationDuration => hesitationDuration;
    public float LanternResistance => lanternResistance;
    public float SearchDuration => searchDuration;
    public float StalkingPressure => stalkingPressure;
}

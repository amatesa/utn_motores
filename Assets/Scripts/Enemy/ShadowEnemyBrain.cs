using UnityEngine;

/// <summary>
/// Orquestador SRP del enemigo.
///
/// RESPONSABILIDAD:
/// - Mantener la FSM (Idle / Investigate / Chase / Retreat)
/// - Coordinar módulos externos (visión, hearing, movimiento, teleporte, puertas, visual)
///
/// NOTA:
/// Este script es la puerta de migración para reemplazar gradualmente ShadowEnemy.cs.
/// </summary>
[RequireComponent(typeof(ShadowEnemyMovementController))]
[RequireComponent(typeof(ShadowEnemyHearingSensor))]
[RequireComponent(typeof(ShadowEnemyVisualController))]
[RequireComponent(typeof(ShadowEnemyTeleportController))]
[RequireComponent(typeof(ShadowEnemyDoorInteractor))]
[RequireComponent(typeof(EnemyPresenceAudio))]
[RequireComponent(typeof(EnemyPerception))]


public class ShadowEnemyBrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private EnemyPerception perception;

    [Header("State Timers")]
    [SerializeField] private float investigateDuration = 4f;
    [SerializeField] private float loseSightTime = 2f;
    [SerializeField] private float hesitationTime = 2f;
    [SerializeField] private float fakeRetreatChance = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    private ShadowEnemyMovementController movement;
    private ShadowEnemyHearingSensor hearing;
    private ShadowEnemyVisualController visual;
    private ShadowEnemyTeleportController teleport;
    private ShadowEnemyDoorInteractor doorInteractor;

    private float investigateTimer;
    private float loseSightTimer;
    private float hesitationTimer;
    private bool isHesitating;
    private Vector3 lastKnownPosition;

    private void Awake()
    {
        movement = GetComponent<ShadowEnemyMovementController>();
        hearing = GetComponent<ShadowEnemyHearingSensor>();
        visual = GetComponent<ShadowEnemyVisualController>();
        teleport = GetComponent<ShadowEnemyTeleportController>();
        doorInteractor = GetComponent<ShadowEnemyDoorInteractor>();

        if (perception == null)
            perception = GetComponent<EnemyPerception>();
    }

    private void Start()
    {
        hearing.ClearStaleContextOnStart();
        visual.Initialize();
        Log("START -> IDLE");
    }

    private void Update()
    {
        bool playerSafe = PlayerSafeState.Instance != null && PlayerSafeState.Instance.IsSafe;

        if (playerSafe && CurrentState == EnemyState.Chase)
        {
            SwitchToRetreat();
        }

        if (!playerSafe && perception != null && perception.CanSeePlayer() && CurrentState != EnemyState.Chase)
        {
            SwitchToChase();
        }

        movement.TickVariation(CurrentState);
        doorInteractor?.TryInteractAhead(CurrentState);

        switch (CurrentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Investigate:
                HandleInvestigate();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Retreat:
                HandleRetreat();
                break;
        }

        visual.Tick();
    }

    private void HandleIdle()
    {
        var sound = hearing.GetRelevantSound(transform.position, gameObject);
        if (sound.HasValue)
        {
            lastKnownPosition = sound.Value.position;
            investigateTimer = 0f;
            CurrentState = EnemyState.Investigate;
            Log("IDLE -> INVESTIGATE");
            return;
        }

        movement.Patrol();

        if (teleport != null && teleport.CanTeleport(CurrentState, target, perception, transform.position, gameObject))
            teleport.TryTeleport(target, gameObject);
    }

    private void HandleInvestigate()
    {
        investigateTimer += Time.deltaTime;

        var sound = hearing.GetRelevantSound(transform.position, gameObject);
        if (sound.HasValue)
            lastKnownPosition = sound.Value.position;

        movement.MoveTo(lastKnownPosition);

        if (movement.Reached(1f))
            movement.SearchAround(lastKnownPosition, 2f);

        if (investigateTimer > investigateDuration)
        {
            CurrentState = EnemyState.Idle;
            Log("INVESTIGATE -> IDLE");
        }
    }

    private void HandleChase()
    {
        if (target != null && perception != null && perception.CanSeePlayer())
        {
            loseSightTimer = 0f;
            movement.MoveTo(target.position);
            Log("CHASE -> following player");
            return;
        }

        loseSightTimer += Time.deltaTime;
        if (loseSightTimer > loseSightTime)
        {
            CurrentState = EnemyState.Investigate;
            investigateTimer = 0f;
            Log("CHASE -> INVESTIGATE");
        }
    }

    private void HandleRetreat()
    {
        if (target == null)
            return;

        if (isHesitating)
        {
            hesitationTimer += Time.deltaTime;
            movement.Stop(true);
            movement.LookAt(target.position, transform);

            if (hesitationTimer > hesitationTime)
            {
                isHesitating = false;
                movement.Stop(false);

                if (perception != null && perception.CanSeePlayer() && Random.value < fakeRetreatChance)
                {
                    CurrentState = EnemyState.Chase;
                    Log("FAKE RETREAT -> RE-CHASE");
                    return;
                }

                movement.RetreatFrom(target.position, transform.position, 8f);
                Log("RETREAT -> MOVING AWAY");
            }

            return;
        }

        if (movement.Reached(1f))
        {
            CurrentState = EnemyState.Investigate;
            investigateTimer = 0f;
            Log("RETREAT -> INVESTIGATE");
        }
    }

    private void SwitchToChase()
    {
        CurrentState = EnemyState.Chase;
        Log("CHASE");
    }

    private void SwitchToRetreat()
    {
        CurrentState = EnemyState.Retreat;
        hesitationTimer = 0f;
        isHesitating = true;
        Log("-> RETREAT (hesitating)");
    }

    public bool IsThreatActive() => CurrentState == EnemyState.Chase;

    private void Log(string msg)
    {
        if (!debugEnabled) return;
        Debug.Log($"[ENEMY][{CurrentState}] {msg}");
    }
}

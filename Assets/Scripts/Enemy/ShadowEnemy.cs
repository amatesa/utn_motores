using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Investigate,
    Chase,
    Retreat
}

public class ShadowEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Hearing")]
    [SerializeField] private float investigateDuration = 4f;

    [Header("Chase")]
    [SerializeField] private float loseSightTime = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    [Header("Retreat Behavior")]
    [SerializeField] private float hesitationTime = 2f;
    [SerializeField] private float fakeRetreatChance = 0.3f;

    [Header("Movement Variation")]
    [SerializeField] private float minSpeed = 2.5f;
    [SerializeField] private float maxSpeed = 4.5f;
    [SerializeField] private float speedChangeInterval = 2f;

    [Header("Pause System")]
    [SerializeField] private float pauseChance = 0.1f;
    [SerializeField] private float pauseDuration = 1.2f;

    private EnemyState currentState;
    private NavMeshAgent agent;
    private EnemyPerception perception;

    private float investigateTimer;
    private float loseSightTimer;
    private float hesitationTimer;
    private float speedTimer;
    private float pauseTimer;

    private bool isHesitating;
    private bool isPaused;

    private Vector3 lastKnownPosition;
    private float lastDebugSoundTime = -1f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<EnemyPerception>();

        currentState = EnemyState.Idle;

        NoiseSystem.Instance.ClearSounds();

        DebugLog("START → IDLE");
    }

    void Update()
    {
        bool isPlayerSafe = PlayerSafeState.Instance.IsSafe;

        // SAFE AREA → RETREAT
        if (isPlayerSafe && currentState == EnemyState.Chase)
        {
            DebugLog("PLAYER SAFE → RETREAT");
            SwitchToRetreat();
        }

        // VISIÓN (USANDO PERCEPTION)
        if (perception != null && perception.CanSeePlayer() && !isPlayerSafe)
        {
            if (currentState != EnemyState.Chase)
            {
                SwitchToChase();
            }
        }

        // VARIACIÓN DE VELOCIDAD
        speedTimer += Time.deltaTime;
        if (speedTimer > speedChangeInterval)
        {
            speedTimer = 0f;
            agent.speed = Random.Range(minSpeed, maxSpeed);
        }

        // PAUSA SOLO EN ESTADOS PASIVOS
        bool canPause = currentState == EnemyState.Idle || currentState == EnemyState.Investigate;

        if (canPause && !isPaused && Random.value < pauseChance * Time.deltaTime)
        {
            isPaused = true;
            pauseTimer = pauseDuration;
            agent.isStopped = true;

            DebugLog("PAUSE");
        }

        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;

            if (pauseTimer <= 0f)
            {
                isPaused = false;
                agent.isStopped = false;
            }
        }

        switch (currentState)
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
    }

    // =========================
    // STATES
    // =========================

    void HandleIdle()
    {
        var sound = GetRelevantSound();

        if (sound.HasValue)
        {
            lastKnownPosition = sound.Value.position;
            currentState = EnemyState.Investigate;
            investigateTimer = 0f;

            DebugLog("IDLE → INVESTIGATE");
            return;
        }

        Patrol();
    }

    void HandleInvestigate()
    {
        investigateTimer += Time.deltaTime;

        var sound = GetRelevantSound();

        if (sound.HasValue)
        {
            lastKnownPosition = sound.Value.position;
        }

        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            SearchAround();
        }

        if (investigateTimer > investigateDuration)
        {
            DebugLog("INVESTIGATE → IDLE");
            currentState = EnemyState.Idle;
        }
    }

    void HandleChase()
    {
        if (perception != null && perception.CanSeePlayer())
        {
            loseSightTimer = 0f;
            agent.SetDestination(target.position);
            DebugLog("CHASE → following player");
        }
        else
        {
            loseSightTimer += Time.deltaTime;

            if (loseSightTimer > loseSightTime)
            {
                DebugLog("LOST PLAYER → INVESTIGATE");
                currentState = EnemyState.Investigate;
                investigateTimer = 0f;
                lastKnownPosition = target.position;
            }
        }
    }

    void HandleRetreat()
    {
        if (isHesitating)
        {
            hesitationTimer += Time.deltaTime;

            Vector3 lookDir = (target.position - transform.position).normalized;
            lookDir.y = 0;

            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 2f
                );
            }

            agent.isStopped = true;

            if (hesitationTimer > hesitationTime)
            {
                isHesitating = false;
                agent.isStopped = false;

                // FAKE RETREAT (VALIDADO)
                if (Random.value < fakeRetreatChance && perception != null && perception.CanSeePlayer())
                {
                    DebugLog("FAKE RETREAT → RE-CHASE");
                    currentState = EnemyState.Chase;
                    return;
                }

                Vector3 retreatDir = (transform.position - target.position).normalized;
                Vector3 retreatPoint = transform.position + retreatDir * 8f;

                if (NavMesh.SamplePosition(retreatPoint, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }

                DebugLog("RETREAT → MOVING AWAY");
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            DebugLog("RETREAT → INVESTIGATE");
            currentState = EnemyState.Investigate;
            investigateTimer = 0f;
        }
    }

    // =========================
    // TRANSITIONS
    // =========================

    void SwitchToChase()
    {
        currentState = EnemyState.Chase;
        DebugLog("→ CHASE");
    }

    void SwitchToRetreat()
    {
        currentState = EnemyState.Retreat;

        hesitationTimer = 0f;
        isHesitating = true;

        DebugLog("→ RETREAT (hesitating)");
    }

    // =========================
    // HEARING
    // =========================

    float GetHearingRange(float intensity)
    {
        return Mathf.Sqrt(intensity) * 10f;
    }

    SoundEvent? GetRelevantSound()
    {
        var events = NoiseSystem.Instance.GetSoundEvents();

        SoundEvent? latest = null;

        foreach (var e in events)
        {
            float distance = Vector3.Distance(transform.position, e.position);
            float range = GetHearingRange(e.intensity);

            if (distance > range) continue;

            if (!latest.HasValue || e.time > latest.Value.time)
                latest = e;
        }

        if (latest.HasValue && latest.Value.time != lastDebugSoundTime)
        {
            lastDebugSoundTime = latest.Value.time;
            DebugLog("HEARD SOUND");
        }

        return latest;
    }

    // =========================
    // MOVEMENT
    // =========================

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                DebugLog("PATROL");
            }
        }
    }

    void SearchAround()
    {
        Vector3 randomPoint = lastKnownPosition + Random.insideUnitSphere * 2f;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            DebugLog("SEARCH");
        }
    }

    // =========================
    // DEBUG
    // =========================

    void DebugLog(string msg)
    {
        if (!debugEnabled) return;
        Debug.Log($"[ENEMY][{currentState}] {msg}");
    }
}

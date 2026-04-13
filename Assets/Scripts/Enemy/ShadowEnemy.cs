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

    [Header("FOV")]
    [SerializeField] private float viewAngle = 90f;

    [Header("Vision")]
    [SerializeField] private float visionRange = 6f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask playerMask;

    [Header("Hearing")]
    [SerializeField] private float investigateDuration = 4f;

    [Header("Chase")]
    [SerializeField] private float loseSightTime = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private EnemyState currentState;
    private NavMeshAgent agent;

    private float investigateTimer;
    private float loseSightTimer;

    private Vector3 lastKnownPosition;
    private float lastDebugSoundTime = -1f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.Idle;

        NoiseSystem.Instance.ClearSounds();
        DebugLog("START → IDLE");
    }

    void Update()
    {
        bool isPlayerSafe = PlayerSafeState.Instance.IsSafe;

        // SAFE AREA → FORZAR RETREAT
        if (isPlayerSafe && currentState == EnemyState.Chase)
        {
            DebugLog("PLAYER SAFE → RETREAT");
            SwitchToRetreat();
        }

        // VISIÓN
        if (CanSeePlayer() && !isPlayerSafe)
        {
            SwitchToChase();
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
        if (CanSeePlayer())
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
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            DebugLog("RETREAT → INVESTIGATE");

            Vector3 randomDir = Random.insideUnitSphere * 8f;
            randomDir.y = 0;

            Vector3 newPoint = transform.position + randomDir;

            if (NavMesh.SamplePosition(newPoint, out NavMeshHit hit, 8f, NavMesh.AllAreas))
            {
                lastKnownPosition = hit.position;
            }

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

        Vector3 retreatDir = (transform.position - target.position).normalized;
        Vector3 retreatPoint = transform.position + retreatDir * 6f;

        if (NavMesh.SamplePosition(retreatPoint, out NavMeshHit hit, 6f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        DebugLog("→ RETREAT");
    }

    // =========================
    // VISION
    // =========================

    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * 1f;

        Vector3 dir = (targetPos - origin).normalized;
        float distance = Vector3.Distance(origin, targetPos);

        if (distance > visionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle / 2f)
            return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, visionRange, obstacleMask | playerMask))
        {
            if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
            {
                DebugLog("VISION DETECTED");
                return true;
            }
        }

        Debug.DrawRay(origin, dir * visionRange, Color.red);
        return false;
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

    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, visionRange);

        Vector3 forward = transform.forward;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + forward * visionRange);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + left * visionRange);
        Gizmos.DrawLine(origin, origin + right * visionRange);
    }
}

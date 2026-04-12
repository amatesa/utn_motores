using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Investigate,
    Chase
}

public class ShadowEnemy : MonoBehaviour
{
    public Transform target;

    private EnemyState currentState;
    private Vector3 lastKnownPosition;

    [Header("Hearing")]
    [SerializeField] private float investigateDuration = 4f;

    private float investigateTimer = 0f;

    private NavMeshAgent agent;

    // =========================
    // DEBUG CONTROL
    // =========================
    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

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

        // SAFE AREA (NO BLOQUEA FSM)
        if (isPlayerSafe)
        {
            if (agent.hasPath)
            {
                agent.ResetPath();
                DebugLog("SAFE → ResetPath()");
            }

            investigateTimer = 0f;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle(isPlayerSafe);
                break;

            case EnemyState.Investigate:
                HandleInvestigate(isPlayerSafe);
                break;

            case EnemyState.Chase:
                HandleChase(isPlayerSafe);
                break;
        }
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

        if (events.Count == 0)
            return null;

        SoundEvent? latest = null;

        foreach (var e in events)
        {
            float distance = Vector3.Distance(transform.position, e.position);
            float hearingRange = GetHearingRange(e.intensity);

            if (distance > hearingRange)
                continue;

            if (!latest.HasValue || e.time > latest.Value.time)
            {
                latest = e;
            }
        }

        if (latest.HasValue)
        {
            if (latest.Value.time != lastDebugSoundTime)
            {
                lastDebugSoundTime = latest.Value.time;

                DebugLog($"HEARD SOUND → pos={latest.Value.position} intensity={latest.Value.intensity}");
            }
        }

        return latest;
    }

    // =========================
    // STATES
    // =========================

    void HandleIdle(bool isPlayerSafe)
    {
        var sound = GetRelevantSound();

        if (sound.HasValue && !isPlayerSafe)
        {
            lastKnownPosition = sound.Value.position;
            currentState = EnemyState.Investigate;
            investigateTimer = 0f;

            DebugLog("IDLE → INVESTIGATE");

            return;
        }

        // Patrulla SOLO si no está en safe restriction
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                DebugLog("PATROL → new point");
            }
        }
    }

    void HandleInvestigate(bool isPlayerSafe)
    {
        if (isPlayerSafe)
        {
            DebugLog("INVESTIGATE BLOCKED BY SAFE AREA");
            return;
        }

        investigateTimer += Time.deltaTime;

        var sound = GetRelevantSound();

        if (sound.HasValue)
        {
            lastKnownPosition = sound.Value.position;

            float intensity = sound.Value.intensity;
            agent.speed = Mathf.Lerp(1.5f, 5f, intensity / 20f);

            DebugLog($"UPDATE TARGET → pos={lastKnownPosition} speed={agent.speed}");
        }

        if (!agent.hasPath || Vector3.Distance(agent.destination, lastKnownPosition) > 0.2f)
        {
            agent.SetDestination(lastKnownPosition);
            DebugLog("SET DESTINATION → lastKnownPosition");
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            DebugLog("REACHED LAST POSITION");
        }

        if (investigateTimer > investigateDuration)
        {
            DebugLog("LOST TARGET → BACK TO IDLE");

            currentState = EnemyState.Idle;
            investigateTimer = 0f;
        }
    }

    void HandleChase(bool isPlayerSafe)
    {
        if (isPlayerSafe)
        {
            DebugLog("CHASE BLOCKED BY SAFE AREA");
            return;
        }

        agent.SetDestination(target.position);
        DebugLog($"CHASE → target={target.position}");
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

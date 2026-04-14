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

    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform[] shadowLayers;

    [Header("Shadow Life")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.15f;
    [SerializeField] private float deformSpeed = 1.3f;
    [SerializeField] private float deformAmount = 0.1f;
    [SerializeField] private float driftAmount = 0.2f;
    private Vector3 baseScale;
    private Vector3 basePosition;
    [SerializeField] private float randomScaleAmount = 0.3f;
    [SerializeField] private float randomRotationAmount = 30f;
    [SerializeField] private float randomOffsetAmount = 0.3f;
    private float[] layerSeeds;

    private bool isThreatActive;

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
        baseScale = visualRoot.localScale;
        basePosition = visualRoot.localPosition;
        layerSeeds = new float[shadowLayers.Length];

        for (int i = 0; i < shadowLayers.Length; i++)
        {
            layerSeeds[i] = Random.Range(0f, 100f);
        }

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

        ApplyShadowLife();
    }
    // =========================
    // SHADOW LIFE
    // =========================
    void ApplyShadowLife()
    {
        float time = Time.time;

        for (int i = 0; i < shadowLayers.Length; i++)
        {
            Transform layer = shadowLayers[i];
            float seed = layerSeeds[i];

            float t = time + seed;

            //escala orgánica
            float scaleX = 1 + Mathf.Sin(t * pulseSpeed) * pulseAmount;
            float scaleY = 1 + Mathf.Cos(t * deformSpeed) * deformAmount;
            float scaleZ = 1 + Mathf.Sin(t * deformSpeed * 0.7f) * deformAmount;

            layer.localScale = new Vector3(scaleX, scaleY, scaleZ);

            //offset fluido
            Vector3 offset = new Vector3(
                Mathf.Sin(t * 1.3f),
                0,
                Mathf.Cos(t * 1.1f)
            ) * randomOffsetAmount;

            layer.localPosition = offset;

            //rotación suave
            float rot = Mathf.Sin(t * 0.5f) * randomRotationAmount;
            layer.localRotation = Quaternion.Euler(0, rot, 0);
        }
    }

    // =========================
    // STATES
    // =========================

    public bool IsChasing()
    {
        return currentState == EnemyState.Chase;
    }
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
                isThreatActive = false;
                currentState = EnemyState.Investigate;
            }
        }
    }
    public bool IsThreatActive()
    {
        return isThreatActive;
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
        isThreatActive = true;

        Debug.Log("ENTER CHASE");

        DebugLog("CHASE");
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

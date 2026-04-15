using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Define los estados principales del enemigo.
/// 
/// DISEÑO:
/// FSM simple que controla el flujo de comportamiento.
/// Cada estado representa una intención clara:
/// - Idle → sin estímulos
/// - Investigate → responde a sonido
/// - Chase → persigue jugador
/// - Retreat → se aleja (safe zone / comportamiento psicológico)
/// </summary>
public enum EnemyState
{
    Idle,
    Investigate,
    Chase,
    Retreat
}

/// <summary>
/// Controlador principal del enemigo.
///
/// RESPONSABILIDAD:
/// Orquestar todo el comportamiento del enemigo:
/// - Estados (FSM)
/// - Movimiento (NavMesh)
/// - Percepción (visión + sonido)
/// - Variaciones (pausas, velocidad)
/// - Comportamiento avanzado (retreat, fake retreat)
/// - Feedback visual (shadow life)
///
/// DISEÑO:
/// Este script es un "controlador central" (God Object controlado).
/// Se mantiene así intencionalmente para:
/// - facilitar iteración rápida
/// - validar gameplay completo en un solo lugar
///
/// FLUJO GENERAL:
/// 1. Evaluar condiciones globales (safe area, visión)
/// 2. Aplicar variaciones (speed, pausas)
/// 3. Ejecutar FSM (Idle, Investigate, Chase, Retreat)
/// 4. Aplicar feedback visual (shadow life)
///
/// FUTURO:
/// Este script debería separarse en:
/// - EnemyBrain (FSM)
/// - EnemyMovement
/// - EnemyHearing
/// - EnemyVisual
///
/// IMPORTANTE:
/// No se separa ahora para no romper comportamiento validado.
/// </summary>
public class ShadowEnemy : MonoBehaviour
{
    // =========================
    // TARGET
    // =========================
    [Header("Target")]
    public Transform target;

    // =========================
    // CONFIGURACIÓN DE COMPORTAMIENTO
    // =========================

    [Header("Hearing")]
    // Tiempo máximo que investiga un sonido antes de abandonar
    [SerializeField] private float investigateDuration = 4f;

    [Header("Chase")]
    // Tiempo que tarda en "olvidar" al jugador cuando pierde visión
    [SerializeField] private float loseSightTime = 2f;

    [Header("Retreat Behavior")]
    // Tiempo de duda antes de retirarse (genera tensión)
    [SerializeField] private float hesitationTime = 2f;

    // Probabilidad de cancelar retreat y volver a perseguir
    [SerializeField] private float fakeRetreatChance = 0.3f;

    [Header("Movement Variation")]
    // Variación de velocidad para evitar comportamiento robótico
    [SerializeField] private float minSpeed = 2.5f;
    [SerializeField] private float maxSpeed = 4.5f;
    [SerializeField] private float speedChangeInterval = 2f;

    [Header("Pause System")]
    // Sistema de pausas para dar sensación orgánica
    [SerializeField] private float pauseChance = 0.1f;
    [SerializeField] private float pauseDuration = 1.2f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    [Header("Teleport System")]
    [SerializeField] private float teleportMinDistance = 6f;
    [SerializeField] private float teleportMaxDistance = 12f;
    [SerializeField] private float teleportTriggerDistance = 15f;
    [SerializeField] private float teleportNoStimulusTime = 10f;
    [SerializeField] private int teleportMaxAttempts = 10;
    [SerializeField] private float teleportHeightOffset = 1.2f;

    // =========================
    // VISUAL (SHADOW LIFE)
    // =========================
    // Esta sección controla el feedback visual del enemigo, de momento es una capsula, cuando se cambie el enemy se puede
    // ajustar para que afecte a las partes que se quieran destacar (ej: sombras, partes del cuerpo, etc)
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform[] shadowLayers;

    [Header("Shadow Life")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.15f;
    [SerializeField] private float deformSpeed = 1.3f;
    [SerializeField] private float deformAmount = 0.1f;
    [SerializeField] private float randomRotationAmount = 30f;
    [SerializeField] private float randomOffsetAmount = 0.3f;

    // =========================
    // ESTADO INTERNO
    // =========================

    private EnemyState currentState;
    private NavMeshAgent agent;
    private EnemyPerception perception;

    // =========================
    // TIMERS
    // =========================

    private float investigateTimer;
    private float loseSightTimer;
    private float hesitationTimer;
    private float speedTimer;
    private float pauseTimer;

    private bool isHesitating;
    private bool isPaused;
    private float noStimulusTimer;

    // =========================
    // HEARING
    // =========================

    private Vector3 lastKnownPosition;
    private float lastDebugSoundTime = -1f;

    // =========================
    // VISUAL
    // =========================

    private float[] layerSeeds;

    // =========================
    // INIT
    // =========================
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<EnemyPerception>();

        currentState = EnemyState.Idle;

        // Inicialización visual (para variación orgánica)
        layerSeeds = new float[shadowLayers.Length];
        for (int i = 0; i < shadowLayers.Length; i++)
        {
            layerSeeds[i] = Random.Range(0f, 100f);
        }

        // Limpia sonidos iniciales para evitar comportamientos erráticos
        NoiseSystem.Instance.ClearSounds();

        DebugLog("START → IDLE");
    }

    // =========================
    // UPDATE LOOP
    // =========================
    void Update()
    {
        bool isPlayerSafe = PlayerSafeState.Instance.IsSafe;

        if (isPlayerSafe && currentState == EnemyState.Chase)
        {
            DebugLog("PLAYER SAFE → RETREAT");
            SwitchToRetreat();
        }

        if (perception != null && perception.CanSeePlayer() && !isPlayerSafe)
        {
            if (currentState != EnemyState.Chase)
            {
                SwitchToChase();
            }
        }

        speedTimer += Time.deltaTime;
        if (speedTimer > speedChangeInterval)
        {
            speedTimer = 0f;
            agent.speed = Random.Range(minSpeed, maxSpeed);
        }

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

        // =========================
        // DETECCIÓN DE PUERTAS
        // =========================
        if (currentState == EnemyState.Chase || currentState == EnemyState.Investigate)
        {
            HandleDoorDetection();
        }

        // =========================
        // FSM
        // =========================
        UpdateNoStimulusTimer();

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
    // STATES
    // =========================

    void HandleIdle()
    {
        // Busca estímulos auditivos
        var sound = GetRelevantSound();

        if (sound.HasValue)
        {
            lastKnownPosition = sound.Value.position;
            currentState = EnemyState.Investigate;
            investigateTimer = 0f;

            DebugLog("IDLE → INVESTIGATE");
            return;
        }

        // Sin estímulos → patrulla
        Patrol();

        if (CanTeleport())
        {
            TryTeleport();
        }
    }

    void UpdateNoStimulusTimer()
    {
        bool hasStimulus = false;

        if (perception != null && perception.CanSeePlayer())
            hasStimulus = true;

        if (GetRelevantSound().HasValue)
            hasStimulus = true;

        if (hasStimulus)
            noStimulusTimer = 0f;
        else
            noStimulusTimer += Time.deltaTime;
    }

    bool CanTeleport()
    {
        if (currentState != EnemyState.Idle)
            return false;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        bool farEnough = distanceToPlayer > teleportTriggerDistance;
        bool noStimulus = noStimulusTimer > teleportNoStimulusTime;

        return farEnough || noStimulus;
    }

    void TryTeleport()
    {
        for (int i = 0; i < teleportMaxAttempts; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere.normalized;
            randomDir.y = 0;

            float distance = Random.Range(teleportMinDistance, teleportMaxDistance);
            Vector3 candidate = target.position + randomDir * distance;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;

            if (IsVisibleToPlayer(hit.position))
                continue;

            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                continue;

            ExecuteTeleport(hit.position);
            return;
        }

        DebugLog("TELEPORT FAILED");
    }

    bool IsVisibleToPlayer(Vector3 position)
    {
        if (target == null) return true;

        Vector3 origin = target.position + Vector3.up * 1.5f;
        Vector3 dir = (position - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, 50f))
        {
            return hit.collider.gameObject == gameObject;
        }

        return false;
    }

    void ExecuteTeleport(Vector3 position)
    {
        DebugLog("TELEPORT");

        agent.Warp(position);

        noStimulusTimer = 0f;
    }

    void HandleInvestigate()
    {
        investigateTimer += Time.deltaTime;

        // Actualiza target dinámicamente si hay nuevos sonidos
        var sound = GetRelevantSound();
        if (sound.HasValue)
        {
            lastKnownPosition = sound.Value.position;
        }

        agent.SetDestination(lastKnownPosition);

        // Comportamiento de búsqueda al llegar
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            SearchAround();
        }

        // Timeout → vuelve a idle
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
            // Mantiene persecución activa
            loseSightTimer = 0f;
            agent.SetDestination(target.position);

            DebugLog("CHASE → following player");
        }
        else
        {
            // Pierde visión progresivamente
            loseSightTimer += Time.deltaTime;

            if (loseSightTimer > loseSightTime)
            {
                currentState = EnemyState.Investigate;
            }
        }
    }

    void HandleRetreat()
    {
        // Fase de duda → genera tensión psicológica
        if (isHesitating)
        {
            hesitationTimer += Time.deltaTime;

            agent.isStopped = true;

            // Mira al jugador mientras duda
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

            if (hesitationTimer > hesitationTime)
            {
                isHesitating = false;
                agent.isStopped = false;

                // Fake retreat → comportamiento impredecible
                if (Random.value < fakeRetreatChance && perception.CanSeePlayer())
                {
                    DebugLog("FAKE RETREAT → RE-CHASE");
                    currentState = EnemyState.Chase;
                    return;
                }

                // Movimiento de escape
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

        // Al terminar retreat → vuelve a investigar
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
        
        DebugLog("CHASE");
    }

    void SwitchToRetreat()
    {
        currentState = EnemyState.Retreat;

        hesitationTimer = 0f;
        isHesitating = true;

        DebugLog("→ RETREAT (hesitating)");
    }
    public bool IsThreatActive()
    {
        return currentState == EnemyState.Chase;
    }

    // =========================
    // HEARING
    // =========================

    // Convierte intensidad de sonido en rango perceptible
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
            //IGNORAR eventos generados por este mismo enemigo
            if (e.source == gameObject)
                continue;

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
    // VISUAL
    // =========================

    void ApplyShadowLife()
    {
        float time = Time.time;

        for (int i = 0; i < shadowLayers.Length; i++)
        {
            Transform layer = shadowLayers[i];
            float seed = layerSeeds[i];

            float t = time + seed;

            float scaleX = 1 + Mathf.Sin(t * pulseSpeed) * pulseAmount;
            float scaleY = 1 + Mathf.Cos(t * deformSpeed) * deformAmount;
            float scaleZ = 1 + Mathf.Sin(t * deformSpeed * 0.7f) * deformAmount;

            layer.localScale = new Vector3(scaleX, scaleY, scaleZ);

            Vector3 offset = new Vector3(
                Mathf.Sin(t * 1.3f),
                0,
                Mathf.Cos(t * 1.1f)
            ) * randomOffsetAmount;

            layer.localPosition = offset;

            float rot = Mathf.Sin(t * 0.5f) * randomRotationAmount;
            layer.localRotation = Quaternion.Euler(0, rot, 0);
        }
    }

    private void HandleDoorDetection()
    {
        // Solo tiene sentido si el agente se está moviendo
        if (agent.velocity.magnitude < 0.1f) return;

        RaycastHit hit;

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out hit, 1.5f))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                DebugLog("DOOR DETECTED → INTERACT");

                interactable.InteractFromEnemy(gameObject);
            }
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

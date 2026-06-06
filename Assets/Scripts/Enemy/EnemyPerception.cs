using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sistema de visión del enemigo.
///
/// RESPONSABILIDAD:
/// Determinar si el enemigo puede ver al jugador.
///
/// INTERACCIONES:
/// - Lee: Transform del jugador (target)
/// - Es usado por: ShadowEnemy (para tomar decisiones, hasta hacer refactor de ShadowEnemy)
///
/// DISEÑO:
/// Este sistema NO decide comportamiento, solo responde:
/// → "¿Puedo ver al jugador?"
///
/// PIPELINE DE DETECCIÓN:
/// 1. Distancia (rápido)
/// 2. Ángulo (medio)
/// 3. Raycast (costoso)
///
/// Esto optimiza performance evitando raycasts innecesarios.
///
/// DETALLES DE DISEÑO:
/// - Visión dinámica: más cerca = mayor campo visual
/// - Usa raycast para evitar ver a través de paredes
/// - Debug visual para facilitar testing
/// </summary>
public class EnemyPerception : MonoBehaviour
{
    // =========================
    // TARGET
    // =========================
    // Enemy sabe donde está el jugador, pero no necesariamente lo ve
    public Transform target;

    // =========================
    // CONFIGURACIÓN VISIÓN
    // =========================
    [Header("Vision")]
    public float visionRange = 8f;
    public float viewAngle = 120f;

    [Header("Navigation Range")]
    [SerializeField] private bool requireNavigablePath = true;
    [SerializeField] private float navMeshSampleRadius = 1.5f;
    [SerializeField] private float maxNavMeshSampleVerticalOffset = 0.75f;
    [SerializeField] private float pathDistanceTolerance = 0.25f;

    // =========================
    // LAYERS
    // =========================
    [Header("Layers")]
    // obstacleMask en este momento es Default, pero separamos para flexibilidad futura
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    // =========================
    // DEBUG
    // =========================
    [Header("Debug")]
    public bool debug = true;

    private NavMeshPath reusablePath;

    private void Awake()
    {
        reusablePath = new NavMeshPath();
    }

    /// <summary>
    /// Evalúa si el enemigo puede ver al jugador.
    /// </summary>
    ///

    public bool CanSeePlayer()
    {
        
        // Sin target no hay nada que detectar
        if (target == null) return false;

        // Si el player esta escondido no lo detecta
        if (PlayerHideState.Instance != null && PlayerHideState.Instance.IsHidden)
        {
            if (debug)
                Debug.Log("[PERCEPTION] PLAYER HIDDEN");

            return false;
        }

        // Se eleva el origen para evitar problemas con el suelo
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        // Se apunta al centro del jugador (no a los pies)
        Vector3 targetPos = target.position + Vector3.up;

        // Dirección y distancia al target normalizadas (para checks posteriores)
        Vector3 dir = (targetPos - origin).normalized;
        float directDistance = Vector3.Distance(origin, targetPos);

        // =========================
        // NAVIGATION DISTANCE CHECK
        // =========================
        // El rango se valida por camino navegable, no por distancia 3D directa.
        // Evita falsos positivos cuando el player está justo encima/debajo en otro piso.
        if (!TryGetReachableDistanceToTarget(visionRange, out float reachableDistance))
        {
            DebugDraw(origin, dir, Color.gray);
            return false;
        }

        // =========================
        // ANGLE CHECK
        // =========================
        float angle = Vector3.Angle(transform.forward, dir);

        // VISIÓN DINÁMICA:
        // Cuando el jugador está cerca, el enemigo "percibe más"
        // Esto evita situaciones frustrantes donde el jugador está
        // muy cerca pero fuera del ángulo exacto.
        float effectiveAngle = viewAngle;

        if (reachableDistance < 3f)
        {
            effectiveAngle *= 1.5f;
        }

        // Si está fuera del cono de visión → no visible
        if (angle > effectiveAngle / 2f)
        {
            DebugDraw(origin, dir, Color.yellow);
            return false;
        }

        // =========================
        // RAYCAST (VISIBILITY CHECK)
        // =========================
        // Confirma que no hay obstáculos entre enemigo y el punto objetivo.
        // IMPORTANTE:
        // En FPS el target suele ser un punto/cámara sin collider, por eso no podemos
        // depender de "pegarle" al layer del player para considerar visión válida.
        // Si no hay obstáculo en la línea de visión hasta el target, el jugador es visible.
        if (Physics.Raycast(origin, dir, out RaycastHit obstacleHit, directDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            DebugDraw(origin, dir, Color.red);
            Debug.Log("[PERCEPTION] BLOCKED BY " + obstacleHit.collider.name);
            return false;
        }

        // Debug opcional: intentamos verificar si hay collider del player en la trayectoria,
        // pero ya no es requisito para detectar (evita falsos negativos en modo FPS).
        if (playerMask.value != 0 && Physics.Raycast(origin, dir, out RaycastHit playerHit, directDistance, playerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("[PERCEPTION] PLAYER COLLIDER HIT: " + playerHit.collider.name);
        }

        DebugDraw(origin, dir, Color.green);
        Debug.Log("[PERCEPTION] PLAYER DETECTED");
        return true;
    }

    public bool IsTargetReachableWithin(float maxDistance)
    {
        return TryGetReachableDistanceToTarget(maxDistance, out _);
    }

    public bool TryGetReachableDistanceToTarget(float maxDistance, out float reachableDistance)
    {
        if (!TryGetReachablePathToTarget(out reachableDistance, out _))
            return false;

        return reachableDistance <= maxDistance + pathDistanceTolerance;
    }

    public bool TryGetReachablePathToTarget(out float reachableDistance, out Vector3 navMeshTargetPosition)
    {
        reachableDistance = Mathf.Infinity;
        navMeshTargetPosition = Vector3.zero;

        if (target == null)
            return false;

        if (!requireNavigablePath)
        {
            reachableDistance = Vector3.Distance(transform.position, target.position);
            navMeshTargetPosition = target.position;
            return true;
        }

        if (reusablePath == null)
            reusablePath = new NavMeshPath();

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit selfHit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            if (debug)
                Debug.Log("[PERCEPTION] ENEMY NOT ON NAVMESH");

            return false;
        }

        if (Mathf.Abs(transform.position.y - selfHit.position.y) > maxNavMeshSampleVerticalOffset)
        {
            if (debug)
                Debug.Log("[PERCEPTION] ENEMY NAVMESH SAMPLE ON DIFFERENT FLOOR");

            return false;
        }

        if (!NavMesh.SamplePosition(target.position, out NavMeshHit targetHit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            if (debug)
                Debug.Log("[PERCEPTION] PLAYER NOT ON NAVMESH");

            return false;
        }

        if (Mathf.Abs(target.position.y - targetHit.position.y) > maxNavMeshSampleVerticalOffset)
        {
            if (debug)
                Debug.Log("[PERCEPTION] PLAYER NAVMESH SAMPLE ON DIFFERENT FLOOR");

            return false;
        }

        navMeshTargetPosition = targetHit.position;

        if (!NavMesh.CalculatePath(selfHit.position, targetHit.position, NavMesh.AllAreas, reusablePath) ||
            reusablePath.status != NavMeshPathStatus.PathComplete)
        {
            if (debug)
                Debug.Log("[PERCEPTION] PLAYER NAV PATH INCOMPLETE");

            return false;
        }

        reachableDistance = GetPathLength(reusablePath);
        return true;
    }

    private float GetPathLength(NavMeshPath path)
    {
        Vector3[] corners = path.corners;
        if (corners == null || corners.Length < 2)
            return 0f;

        float length = 0f;
        for (int i = 1; i < corners.Length; i++)
        {
            length += Vector3.Distance(corners[i - 1], corners[i]);
        }

        return length;
    }

    // =========================
    // DEBUG VISUAL
    // =========================
    void DebugDraw(Vector3 origin, Vector3 dir, Color color)
    {
        if (!debug) return;

        Debug.DrawRay(origin, dir * visionRange, color);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        // Rango de visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, visionRange);

        Vector3 forward = transform.forward;

        // Límites del cono de visión
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + left * visionRange);
        Gizmos.DrawLine(origin, origin + right * visionRange);

        // Dirección frontal
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + forward * visionRange);
    }
}

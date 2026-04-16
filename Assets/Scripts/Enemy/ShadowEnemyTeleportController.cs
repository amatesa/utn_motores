using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Teleport táctico del enemigo cuando no hay estímulos.
/// </summary>
[RequireComponent(typeof(ShadowEnemyMovementController))]
public class ShadowEnemyTeleportController : MonoBehaviour
{
    [Header("Teleport")]
    [SerializeField] private float teleportMinDistance = 6f;
    [SerializeField] private float teleportMaxDistance = 12f;
    [SerializeField] private float teleportTriggerDistance = 15f;
    [SerializeField] private float teleportNoStimulusTime = 10f;
    [SerializeField] private int teleportMaxAttempts = 10;

    [Header("Hit Teleport")]
    [SerializeField] private float hitTeleportMinDistance = 14f;
    [SerializeField] private float hitTeleportMaxDistance = 24f;
    [SerializeField] private int hitTeleportMaxAttempts = 18;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private ShadowEnemyMovementController movement;
    private float noStimulusTimer;

    private void Awake()
    {
        movement = GetComponent<ShadowEnemyMovementController>();
    }

    public bool CanTeleport(EnemyState state, Transform target, EnemyPerception perception, Vector3 selfPosition, GameObject self)
    {
        if (state != EnemyState.Idle || target == null)
            return false;

        bool hasStimulus = false;

        if (perception != null && perception.CanSeePlayer())
            hasStimulus = true;

        if (NoiseSystem.Instance != null)
        {
            foreach (var e in NoiseSystem.Instance.GetSoundEvents())
            {
                if (e.source == self) continue;
                hasStimulus = true;
                break;
            }
        }

        if (hasStimulus)
            noStimulusTimer = 0f;
        else
            noStimulusTimer += Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(selfPosition, target.position);
        bool farEnough = distanceToPlayer > teleportTriggerDistance;
        bool noStimulusTooLong = noStimulusTimer > teleportNoStimulusTime;

        return farEnough || noStimulusTooLong;
    }

    public bool TryTeleport(Transform target, GameObject self)
    {
        return TryTeleportInternal(target, self, teleportMinDistance, teleportMaxDistance, teleportMaxAttempts, true);
    }

    public bool TeleportAfterPlayerHit(Transform player, GameObject self, bool resetTemporalReference = true)
    {
        if (player == null)
            return false;

        bool teleported = TryTeleportInternal(
            player,
            self,
            hitTeleportMinDistance,
            hitTeleportMaxDistance,
            hitTeleportMaxAttempts,
            false
        );

        if (resetTemporalReference)
        {
            // Reinicia referencia temporal para que una nueva colisión
            // no quede bloqueada por temporizadores internos del sistema.
            noStimulusTimer = 0f;
        }

        if (debugEnabled)
        {
            Debug.Log($"[ENEMY][TELEPORT] TELEPORT AFTER PLAYER HIT => {(teleported ? "SUCCESS" : "FAILED")}");
        }

        return teleported;
    }

    private bool TryTeleportInternal(
        Transform target,
        GameObject self,
        float minDistance,
        float maxDistance,
        int maxAttempts,
        bool avoidPlayerVisibility)
    {
        if (target == null)
            return false;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere.normalized;
            randomDir.y = 0f;

            float distance = Random.Range(minDistance, maxDistance);
            Vector3 candidate = target.position + randomDir * distance;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                continue;

            float finalDistance = Vector3.Distance(target.position, hit.position);
            if (finalDistance < minDistance)
                continue;

            if (avoidPlayerVisibility && IsVisibleToPlayer(target, hit.position, self))
                continue;

            if (!movement.TryCalculatePath(hit.position))
                continue;

            movement.Warp(hit.position);
            noStimulusTimer = 0f;

            if (debugEnabled)
                Debug.Log($"[ENEMY][TELEPORT] TELEPORT success at distance={finalDistance:F2}");

            return true;
        }

        if (debugEnabled)
            Debug.LogWarning("[ENEMY][TELEPORT] TELEPORT FAILED after all attempts");

        return false;
    }
       
    private bool IsVisibleToPlayer(Transform player, Vector3 position, GameObject self)
    {
        Vector3 origin = player.position + Vector3.up * 1.5f;
        Vector3 dir = (position - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, 50f))
            return hit.collider.gameObject == self;

        return false;
    }
}

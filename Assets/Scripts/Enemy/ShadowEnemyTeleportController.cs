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

    public void TryTeleport(Transform target, GameObject self)
    {
        if (target == null) return;

        for (int i = 0; i < teleportMaxAttempts; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere.normalized;
            randomDir.y = 0f;

            float distance = Random.Range(teleportMinDistance, teleportMaxDistance);
            Vector3 candidate = target.position + randomDir * distance;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;

            if (IsVisibleToPlayer(target, hit.position, self))
                continue;

            if (!movement.TryCalculatePath(hit.position))
                continue;

            movement.Warp(hit.position);
            noStimulusTimer = 0f;

            if (debugEnabled)
                Debug.Log("[ENEMY][TELEPORT] TELEPORT");

            return;
        }

        if (debugEnabled)
            Debug.Log("[ENEMY][TELEPORT] TELEPORT FAILED");
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

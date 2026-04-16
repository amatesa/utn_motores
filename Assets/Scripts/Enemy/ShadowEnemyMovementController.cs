using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Movimiento y utilidades cinemáticas del enemigo.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ShadowEnemyMovementController : MonoBehaviour
{
    [Header("Movement Variation")]
    [SerializeField] private float minSpeed = 2.5f;
    [SerializeField] private float maxSpeed = 4.5f;
    [SerializeField] private float speedChangeInterval = 2f;

    [Header("Pause System")]
    [SerializeField] private float pauseChance = 0.1f;
    [SerializeField] private float pauseDuration = 1.2f;

    private NavMeshAgent agent;
    private float speedTimer;
    private float pauseTimer;
    private bool isPaused;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void TickVariation(EnemyState state)
    {
        speedTimer += Time.deltaTime;
        if (speedTimer > speedChangeInterval)
        {
            speedTimer = 0f;
            agent.speed = Random.Range(minSpeed, maxSpeed);
        }

        bool canPause = state == EnemyState.Idle || state == EnemyState.Investigate;
        if (canPause && !isPaused && Random.value < pauseChance * Time.deltaTime)
        {
            isPaused = true;
            pauseTimer = pauseDuration;
            agent.isStopped = true;
        }

        if (!isPaused) return;

        pauseTimer -= Time.deltaTime;
        if (pauseTimer <= 0f)
        {
            isPaused = false;
            agent.isStopped = false;
        }
    }

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }

    public void Patrol()
    {
        if (agent.pathPending || agent.remainingDistance >= 0.5f)
            return;

        Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void SearchAround(Vector3 center, float radius)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void RetreatFrom(Vector3 threatPosition, Vector3 selfPosition, float distance)
    {
        Vector3 retreatDir = (selfPosition - threatPosition).normalized;
        Vector3 retreatPoint = selfPosition + retreatDir * distance;

        if (NavMesh.SamplePosition(retreatPoint, out NavMeshHit hit, distance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public bool Reached(float threshold)
    {
        return !agent.pathPending && agent.remainingDistance < threshold;
    }

    public void Stop(bool value)
    {
        agent.isStopped = value;
    }

    public void LookAt(Vector3 targetPosition, Transform actor)
    {
        Vector3 lookDir = (targetPosition - actor.position).normalized;
        lookDir.y = 0f;

        if (lookDir == Vector3.zero) return;

        actor.rotation = Quaternion.Slerp(
            actor.rotation,
            Quaternion.LookRotation(lookDir),
            Time.deltaTime * 2f
        );
    }

    public bool IsMoving(float minVelocity = 0.1f)
    {
        return agent.velocity.magnitude >= minVelocity;
    }

    public bool TryCalculatePath(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        return agent.CalculatePath(destination, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    public void Warp(Vector3 position)
    {
        agent.Warp(position);
    }
}

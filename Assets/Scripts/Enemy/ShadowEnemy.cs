using UnityEngine;
using UnityEngine.AI;

public class ShadowEnemy : MonoBehaviour
{
    public Transform target; // Player
    public float noiseThreshold = 10f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float noise = NoiseSystem.Instance.GetNoise();

        // Si player está en zona segura frena
        if (PlayerSafeState.Instance.isSafe)
        {
            if (agent.hasPath)
            {
                agent.ResetPath();
            }
            return;
        }

        // comportamiento normal
        if (noise > noiseThreshold)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            if (agent.hasPath)
            {
                agent.ResetPath();
            }
        }
    }
}

using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    public Transform target;

    [Header("Vision")]
    public float visionRange = 8f;
    public float viewAngle = 120f; // 🔥 más amplio

    [Header("Layers")]
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Debug")]
    public bool debug = true;

    public bool CanSeePlayer()
    {
        if (target == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up;

        Vector3 dir = (targetPos - origin).normalized;
        float distance = Vector3.Distance(origin, targetPos);

        // 🔹 RANGE CHECK
        if (distance > visionRange)
        {
            DebugDraw(origin, dir, Color.gray);
            return false;
        }

        // ANGLE CHECK
        float angle = Vector3.Angle(transform.forward, dir);

        // VISIÓN DINÁMICA
        float effectiveAngle = viewAngle;

        // más cerca = más visión periférica
        if (distance < 3f)
        {
            effectiveAngle *= 1.5f;
        }

        // chequeo final
        if (angle > effectiveAngle / 2f)
        {
            DebugDraw(origin, dir, Color.yellow);
            return false;
        }

        // 🔹 RAYCAST (IMPORTANTE)
        if (Physics.Raycast(origin, dir, out RaycastHit hit, visionRange, obstacleMask | playerMask))
        {
            if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
            {
                DebugDraw(origin, dir, Color.green);
                Debug.Log("[PERCEPTION] PLAYER DETECTED");
                return true;
            }
            else
            {
                DebugDraw(origin, dir, Color.red);
                Debug.Log("[PERCEPTION] BLOCKED BY " + hit.collider.name);
            }
        }

        return false;
    }

    void DebugDraw(Vector3 origin, Vector3 dir, Color color)
    {
        if (!debug) return;

        Debug.DrawRay(origin, dir * visionRange, color);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, visionRange);

        Vector3 forward = transform.forward;

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + left * visionRange);
        Gizmos.DrawLine(origin, origin + right * visionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + forward * visionRange);
    }
}

using UnityEngine;

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

    /// <summary>
    /// Evalúa si el enemigo puede ver al jugador.
    /// </summary>
    public bool CanSeePlayer()
    {
        // Sin target no hay nada que detectar
        if (target == null) return false;

        // Se eleva el origen para evitar problemas con el suelo
        Vector3 origin = transform.position + Vector3.up * 1.5f;

        // Se apunta al centro del jugador (no a los pies)
        Vector3 targetPos = target.position + Vector3.up;

        // Dirección y distancia al target normalizadas (para checks posteriores)
        Vector3 dir = (targetPos - origin).normalized;
        float distance = Vector3.Distance(origin, targetPos);

        // =========================
        // DISTANCE CHECK
        // =========================
        // Primer filtro > evita cálculos innecesarios
        if (distance > visionRange)
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

        if (distance < 3f)
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
        // Confirma que no hay obstáculos entre enemigo y jugador
        // Funciona así: lanza un rayo desde el enemigo hacia el jugador, y verifica qué es lo primero que impacta.
        // visionRange, obstacleMask | playerMask > el raycast solo detectará colisiones con capas de obstáculos o jugador,
        // ignorando otras capas.
        if (Physics.Raycast(origin, dir, out RaycastHit hit, visionRange, obstacleMask | playerMask))
        {
            // Si el primer impacto es el jugador → visible
            if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
            {
                DebugDraw(origin, dir, Color.green);
                Debug.Log("[PERCEPTION] PLAYER DETECTED");
                return true;
            }
            else
            {
                // Algo bloquea la visión (pared, objeto)
                DebugDraw(origin, dir, Color.red);
                Debug.Log("[PERCEPTION] BLOCKED BY " + hit.collider.name);
            }
        }

        return false;
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

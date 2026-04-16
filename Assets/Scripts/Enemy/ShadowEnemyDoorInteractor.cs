using UnityEngine;

/// <summary>
/// Detección frontal de interactuables para apertura de puertas desde IA.
/// </summary>
[RequireComponent(typeof(ShadowEnemyMovementController))]
public class ShadowEnemyDoorInteractor : MonoBehaviour
{
    [SerializeField] private float interactDistance = 1.5f;
    [SerializeField] private float originHeight = 1.0f;
    [SerializeField] private bool debugEnabled = true;

    private ShadowEnemyMovementController movement;

    private void Awake()
    {
        movement = GetComponent<ShadowEnemyMovementController>();
    }

    public void TryInteractAhead(EnemyState state)
    {
        bool validState = state == EnemyState.Chase || state == EnemyState.Investigate || state == EnemyState.Idle;
        if (!validState) return;

        if (!movement.IsMoving()) return;

        Vector3 origin = transform.position + Vector3.up * originHeight;
        Vector3 direction = transform.forward;

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, interactDistance))
            return;

        Interactable interactable = hit.collider.GetComponent<Interactable>();
        if (interactable == null) return;

        if (debugEnabled)
            Debug.Log("[ENEMY][DOOR] DOOR DETECTED -> INTERACT");

        interactable.InteractFromEnemy(gameObject);
    }
}

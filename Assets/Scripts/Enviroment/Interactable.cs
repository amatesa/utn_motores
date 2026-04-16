using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent onInteract;

    [Header("AI Interaction")]
    [SerializeField] private bool canEnemyInteract = false;

    // NUEVO: Interacción con instigator
    public void Interact(GameObject instigator)
    {
        Debug.Log("[INTERACTABLE] Interaction on: " + gameObject.name + " by " + instigator.name);

        // Intentar emitir sonido contextual
        InteractionEmitter emitter = GetComponentInParent<InteractionEmitter>();
        if (emitter != null)
        {
            emitter.Interact(instigator);
        }

        onInteract?.Invoke();
    }

    // Mantener compatibilidad (opcional)
    public void Interact()
    {
        Interact(gameObject); // fallback (no ideal)
    }

    // Interacción desde Enemy
    public void InteractFromEnemy(GameObject enemy)
    {
        if (!canEnemyInteract) return;

        Debug.Log("[INTERACTABLE] Enemy interaction on: " + gameObject.name);

        InteractionEmitter emitter = GetComponentInParent<InteractionEmitter>();
        if (emitter != null)
        {
            emitter.Interact(enemy);
        }

        Door door = GetComponentInParent<Door>();

        if (door != null)
        {
            door.OpenDoorFromAI(enemy.transform);
        }
        else
        {
            onInteract?.Invoke();
        }
    }
}

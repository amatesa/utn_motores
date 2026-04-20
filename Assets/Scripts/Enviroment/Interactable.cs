using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent onInteract;

    [Header("AI Interaction")]
    [SerializeField] private bool canEnemyInteract = false;

    private HideableSpot hideableSpot;

    private void Awake()
    {
        hideableSpot = GetComponentInParent<HideableSpot>();
    }

    // =========================
    // PLAYER INTERACTION
    // =========================
    public void Interact(GameObject instigator)
    {
        Debug.Log("[INTERACTABLE] Interaction on: " + gameObject.name + " by " + instigator.name);

        // PRIORIDAD: HIDING
        if (hideableSpot != null && instigator.CompareTag("Player"))
        {
            HandleHideInteraction(instigator);
            return;
        }

        // SONIDO
        InteractionEmitter emitter = GetComponentInParent<InteractionEmitter>();
        if (emitter != null)
        {
            emitter.Interact(instigator);
        }

        onInteract?.Invoke();
    }

    private void HandleHideInteraction(GameObject player)
    {
        if (PlayerHideState.Instance == null)
            return;

        if (PlayerHideState.Instance.IsHidden)
        {
            hideableSpot.ExitHide();
        }
        else
        {
            hideableSpot.EnterHide(player);
        }
    }

    // =========================
    // COMPAT
    // =========================
    public void Interact()
    {
        Interact(gameObject);
    }

    // =========================
    // ENEMY INTERACTION
    // =========================
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

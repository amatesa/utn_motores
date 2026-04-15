using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public UnityEvent onInteract;

    [Header("AI Interaction")]
    [SerializeField] private bool canEnemyInteract = false;

    public void Interact()
    {
        Debug.Log("[INTERACTABLE] Player interaction on: " + gameObject.name);
        onInteract?.Invoke();
    }

    // Interacción desde Enemy
    public void InteractFromEnemy(GameObject enemy)
    {
        if (!canEnemyInteract) return;

        Debug.Log("[INTERACTABLE] Enemy interaction on: " + gameObject.name);

        // Buscar Door en el objeto padre
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

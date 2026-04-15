using UnityEngine;

public class EnemyInteractorTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null)
        {
            Debug.Log("[ENEMY] Trigger interact with: " + other.name);

            interactable.InteractFromEnemy(gameObject);
        }
    }
}

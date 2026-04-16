using UnityEngine;
using StarterAssets;

public class InteractorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject interactText;

    private Interactable currentInteractable;
    private StarterAssetsInputs input;

    private void Start()
    {
        input = GetComponentInParent<StarterAssetsInputs>();
    }

    private void Update()
    {
        // Input System (tecla E)
        if (currentInteractable != null && input != null && input.interact)
        {
            Debug.Log("[INTERACTOR] Interacting with: " + currentInteractable.name);

            currentInteractable.Interact(gameObject);

            // Reset para evitar spam
            input.interact = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null)
        {
            Debug.Log("[TRIGGER] Enter: " + other.name);

            currentInteractable = interactable;
            interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null && interactable == currentInteractable)
        {
            Debug.Log("[TRIGGER] Exit: " + other.name);

            currentInteractable = null;
            interactText.SetActive(false);
        }
    }
}

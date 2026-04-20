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
        if (currentInteractable == null || input == null)
            return;

        // Detectar input correctamente
        if (input.interact)
        {
            Debug.Log("[INTERACTOR] Interacting with: " + currentInteractable.name);

            currentInteractable.Interact(transform.root.gameObject);

            // IMPORTANTE: resetear DESPUÉS de usar
            input.interact = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //FIX
        Interactable interactable = other.GetComponentInParent<Interactable>();

        if (interactable != null)
        {
            Debug.Log("[TRIGGER] Enter: " + interactable.name);

            currentInteractable = interactable;
            interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Interactable interactable = other.GetComponentInParent<Interactable>();

        if (interactable != null && interactable == currentInteractable)
        {
            Debug.Log("[TRIGGER] Exit: " + interactable.name);

            currentInteractable = null;
            interactText.SetActive(false);
        }
    }
}

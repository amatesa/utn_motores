using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    private void Start()
    {
        if (itemData == null) return;

        if (ItemStateManager.Instance != null &&
            ItemStateManager.Instance.IsCollected(itemData.itemID))
        {
            Destroy(gameObject);
        }
    }
    public void Collect(GameObject player)
    {
        if (itemData == null)
        {
            Debug.LogError("[ITEM] itemData is NULL on: " + gameObject.name);
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[ITEM] InventoryManager NOT FOUND");
            return;
        }

        // =========================
        // INVENTARIO
        // =========================
        InventoryManager.Instance.AddItem(itemData.itemID);

        Debug.Log("[ITEM] Collected: " + itemData.itemName);

        // =========================
        // FIX UI DIRECTO
        // =========================
        GameObject interactText = GameObject.Find("InteractText");

        if (interactText != null)
        {
            interactText.SetActive(false);
        }

        // =========================
        // DESACTIVAR COLLIDER
        // =========================
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }


        if (ItemStateManager.Instance != null)
        {
            ItemStateManager.Instance.SetCollected(itemData.itemID);
        }
        // =========================
        // DESTROY
        // =========================
        Destroy(gameObject);
    }

    private void SendFakeExit(InteractorTrigger interactor)
    {
        // Hack controlado: desactivar collider antes de destruir
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }
    }
}

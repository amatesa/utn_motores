using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    public void Collect(GameObject player)
    {
        if (itemData == null)
            return;

        InventoryManager.Instance.AddItem(itemData.itemID);

        Debug.Log("[ITEM] Collected: " + itemData.itemName);

        Destroy(gameObject);
    }
}

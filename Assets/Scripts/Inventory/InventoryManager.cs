using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private HashSet<string> collectedItems = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // ADD ITEM
    // =========================
    public void AddItem(string itemID)
    {
        if (collectedItems.Contains(itemID))
            return;

        collectedItems.Add(itemID);
        Debug.Log("[INVENTORY] Added: " + itemID);
    }

    // =========================
    // CHECK ITEM
    // =========================
    public bool HasItem(string itemID)
    {
        return collectedItems.Contains(itemID);
    }

    // =========================
    // REMOVE ITEM
    // =========================
    public void RemoveItem(string itemID)
    {
        if (collectedItems.Contains(itemID))
        {
            collectedItems.Remove(itemID);
            Debug.Log("[INVENTORY] Removed: " + itemID);
        }
    }

    public List<string> GetAllItems()
    {
        return new List<string>(collectedItems);
    }
}

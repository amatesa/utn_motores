using System.Collections.Generic;
using UnityEngine;

public class ItemStateManager : MonoBehaviour
{
    public static ItemStateManager Instance;

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
    public void SetCollected(string itemID)
    {
        if (!collectedItems.Contains(itemID))
        {
            collectedItems.Add(itemID);
            Debug.Log("[ITEM STATE] Saved: " + itemID);
        }
    }

    public bool IsCollected(string itemID)
    {
        return collectedItems.Contains(itemID);
    }
}

using UnityEngine;

public class DoorLock : MonoBehaviour
{
    [Header("Lock Settings")]
    [SerializeField] private bool isLocked = true;
    [SerializeField] private string requiredKeyID;

    [Header("Debug")]
    [SerializeField] private bool debug = true;

    public bool TryUnlock(GameObject instigator)
    {
        if (!isLocked)
            return true;

        if (InventoryManager.Instance == null)
            return false;

        if (InventoryManager.Instance.HasItem(requiredKeyID))
        {
            isLocked = false;

            if (debug)
                Debug.Log("[DOOR] Unlocked with key: " + requiredKeyID);

            return true;
        }

        if (debug)
            Debug.Log("[DOOR] Locked. Missing key: " + requiredKeyID);

        return false;
    }

    public bool IsLocked()
    {
        return isLocked;
    }
}

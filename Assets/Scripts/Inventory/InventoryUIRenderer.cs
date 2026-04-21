using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventoryUIRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject itemSlotPrefab;

    private List<GameObject> currentSlots = new List<GameObject>();

    public void RefreshUI()
    {
        ClearUI();

        if (InventoryManager.Instance == null)
            return;

        List<string> items = InventoryManager.Instance.GetAllItems();

        foreach (string itemID in items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, contentParent);

            // Buscar icono
            ItemData data = FindItemData(itemID);

            if (data != null)
            {
                Image img = slot.transform.Find("Icon")?.GetComponent<Image>();
                if (img != null)
                    img.sprite = data.icon;

                TMP_Text txt = slot.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                    txt.text = data.itemName;
            }

            currentSlots.Add(slot);
        }
    }

    private void ClearUI()
    {
        foreach (var slot in currentSlots)
        {
            Destroy(slot);
        }

        currentSlots.Clear();
    }

    private ItemData FindItemData(string id)
    {
        ItemData[] allItems = Resources.LoadAll<ItemData>("");

        foreach (var item in allItems)
        {
            if (item.itemID == id)
                return item;
        }

        return null;
    }
}

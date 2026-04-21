using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference inventoryAction;

    [Header("UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InventoryUIRenderer uiRenderer;

    private bool isOpen = false;

    private void OnEnable()
    {
        if (inventoryAction != null)
            inventoryAction.action.Enable();
    }

    private void OnDisable()
    {
        if (inventoryAction != null)
            inventoryAction.action.Disable();
    }

    private void Update()
    {
        if (inventoryAction != null && inventoryAction.action.triggered)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    private void OpenInventory()
    {
        isOpen = true;

        inventoryPanel.SetActive(true);

        if (uiRenderer != null)
        {
            uiRenderer.RefreshUI();
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseInventory()
    {
        isOpen = false;

        inventoryPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

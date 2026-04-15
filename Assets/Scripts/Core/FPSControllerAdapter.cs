using UnityEngine;
using UnityEngine.InputSystem;

public class FPSControllerAdapter : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;           // Player (root)
    public Transform cameraRoot;           // PlayerCameraRoot

    [Header("Input")]
    public InputActionReference lookAction;

    [Header("Settings")]
    public float sensitivity = 100f;

    private float xRotation = 0f;

    void OnEnable()
    {
        lookAction.action.Enable();
    }

    void OnDisable()
    {
        lookAction.action.Disable();
    }

    void Update()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        // Rotación vertical (cámara)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotación horizontal (player)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}

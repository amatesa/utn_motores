using UnityEngine;
using UnityEngine.InputSystem;

public class HideCameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float verticalClamp = 60f;

    private float yaw;
    private float pitch;

    private bool isActive = false;

    public void Activate()
    {
        isActive = true;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * sensitivity;
        pitch -= mouseDelta.y * sensitivity;

        pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}

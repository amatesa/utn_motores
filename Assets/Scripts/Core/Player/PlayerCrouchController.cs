using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerCrouchController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraRoot;

    [Header("Input")]
    [SerializeField] private Key crouchKey = Key.C;

    [Header("Standing")]
    [SerializeField] private float standingHeight = 1.4f;
    [SerializeField] private float standingCameraY = 1.2f;

    [Header("Crouching")]
    [SerializeField] private float crouchingHeight = 0.8f;
    [SerializeField] private float crouchingCameraY = 0.8f;

    [Header("Transition")]
    [SerializeField] private float transitionSpeed = 10f;

    private bool isCrouching;

    private Vector3 defaultCameraPosition;

    private void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        defaultCameraPosition = cameraRoot.localPosition;
    }

    private void Update()
    {
        HandleInput();

        UpdateCrouch();
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        isCrouching = Keyboard.current[crouchKey].isPressed;
    }

    private void UpdateCrouch()
    {
        float targetHeight =
            isCrouching ?
            crouchingHeight :
            standingHeight;

        float targetCameraY =
            isCrouching ?
            crouchingCameraY :
            standingCameraY;

        characterController.height =
            Mathf.Lerp(
                characterController.height,
                targetHeight,
                Time.deltaTime * transitionSpeed
            );

        Vector3 center = characterController.center;

        center.y = characterController.height / 2f;

        characterController.center = center;

        Vector3 targetCameraPosition =
            new Vector3(
                defaultCameraPosition.x,
                targetCameraY,
                defaultCameraPosition.z
            );

        cameraRoot.localPosition =
            Vector3.Lerp(
                cameraRoot.localPosition,
                targetCameraPosition,
                Time.deltaTime * transitionSpeed
            );
    }
}

using UnityEngine;
using StarterAssets;

public class HideableSpot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPoint;

    [Header("Settings")]
    [SerializeField] private bool debug = true;

    private GameObject currentPlayer;

    private Transform playerCamera;
    private GameObject playerVisual;

    private Transform originalCameraParent;

    private CharacterController characterController;
    private Collider[] playerColliders;

    [SerializeField] private GameObject hideCamera;
    private GameObject mainCameraObject;
    private HideCameraController hideCameraController;

    private ThirdPersonController thirdPersonController;

    private bool isHiding = false;

    // =========================
    // ENTER HIDE
    // =========================
    public void EnterHide(GameObject player)
    {
        if (isHiding) return;

        currentPlayer = player;

        playerCamera = Camera.main.transform;

        // Buscar visual del player (IMPORTANTE)
        playerVisual = player.transform.Find("Geometry")?.gameObject;

        thirdPersonController = player.GetComponent<ThirdPersonController>();

        // DESACTIVAR cámara principal
        mainCameraObject = Camera.main.gameObject;
        mainCameraObject.SetActive(false);

        // ACTIVAR cámara de escondite
        hideCamera.SetActive(true);

        // DESACTIVAR CONTROL
        if (thirdPersonController != null)
            thirdPersonController.enabled = false;

        // OCULTAR PLAYER
        if (playerVisual != null)
            playerVisual.SetActive(false);

        // GUARDAR PARENT ORIGINAL
        originalCameraParent = playerCamera.parent;

        // MOVER CÁMARA AL ESCONDITE
        playerCamera.SetParent(cameraPoint);
        playerCamera.localPosition = Vector3.zero;
        playerCamera.localRotation = Quaternion.identity;

        characterController = currentPlayer.GetComponent<CharacterController>();
        playerColliders = currentPlayer.GetComponentsInChildren<Collider>();

        hideCameraController = hideCamera.GetComponent<HideCameraController>();

        if (hideCameraController != null)
        {
            hideCameraController.Activate();
        }

        // DESACTIVAR COLLISIONS
        if (characterController != null)
            characterController.enabled = false;

        foreach (var col in playerColliders)
        {
            col.enabled = false;
        }

        // ESTADO
        PlayerHideState.Instance.Hide();

        if (PlayerSafeState.Instance != null)
            PlayerSafeState.Instance.SetSafe(true);

        isHiding = true;

        if (debug)
            Debug.Log("[HIDE] ENTER");
    }

    // =========================
    // EXIT HIDE
    // =========================
    public void ExitHide()
    {
        if (!isHiding) return;

        if (currentPlayer == null) return;

        // REACTIVAR CONTROL
        if (thirdPersonController != null)
            thirdPersonController.enabled = true;

        // MOSTRAR PLAYER
        if (playerVisual != null)
            playerVisual.SetActive(true);

        // NO movemos cámara → StarterAssets la recupera sola

        PlayerHideState.Instance.Unhide();

        if (PlayerSafeState.Instance != null)
            PlayerSafeState.Instance.SetSafe(false);

        isHiding = false;

        if (debug)
            Debug.Log("[HIDE] EXIT");
        // RESTAURAR CÁMARA
        playerCamera.SetParent(originalCameraParent);
        playerCamera.localPosition = Vector3.zero;
        playerCamera.localRotation = Quaternion.identity;

        // REACTIVAR COLLISIONS
        if (characterController != null)
            characterController.enabled = true;

        foreach (var col in playerColliders)
        {
            col.enabled = true;
        }
        // REACTIVAR cámara principal
        if (mainCameraObject != null)
            mainCameraObject.SetActive(true);

        if (hideCameraController != null)
        {
            hideCameraController.Deactivate();
        }

        // DESACTIVAR cámara de escondite
        hideCamera.SetActive(false);

    }

    public void ToggleHide(GameObject player)
    {
        if (isHiding)
            ExitHide();
        else
            EnterHide(player);
    }
}

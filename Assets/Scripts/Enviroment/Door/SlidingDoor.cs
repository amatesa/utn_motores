using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DoorLock))]
public class SlidingDoor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 openDirection = Vector3.forward;
    [SerializeField] private float openDistance = 2f;
    [SerializeField] private float openSpeed = 2f;

    [Header("Persistence")]
    [SerializeField] private string doorID;

    private DoorLock doorLock;

    private bool isOpen;
    private bool isMoving;

    private Vector3 closedPosition;
    private Vector3 openedPosition;

    private void Awake()
    {
        doorLock = GetComponent<DoorLock>();

        closedPosition = transform.localPosition;

        Vector3 direction = openDirection.normalized;
        openedPosition = closedPosition +
                 transform.TransformDirection(openDirection.normalized) * openDistance;

        if (string.IsNullOrWhiteSpace(doorID))
        {
            Debug.LogWarning($"{name}: Door ID está vacío.");
        }

        isOpen = PlayerPrefs.GetInt(GetSaveKey(), 0) == 1;

        if (isOpen)
        {
            transform.localPosition = openedPosition;
        }
    }

    /// <summary>
    /// Compatible con otros sistemas.
    /// </summary>
    public void UseDoor(GameObject instigator)
    {
        if (isOpen || isMoving)
            return;

        if (doorLock != null && !doorLock.TryUnlock(instigator))
            return;

        OpenDoor();
    }

    /// <summary>
    /// Compatible con Interactable -> OnInteract()
    /// </summary>
    public void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning($"{name}: No se encontró un GameObject con tag Player.");
            return;
        }

        UseDoor(player);
    }

    private void OpenDoor()
    {
        isOpen = true;
        isMoving = true;

        PlayerPrefs.SetInt(GetSaveKey(), 1);
        PlayerPrefs.Save();

        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        while (Vector3.Distance(transform.localPosition, openedPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                openedPosition,
                openSpeed * Time.deltaTime);

            yield return null;
        }

        transform.localPosition = openedPosition;
        isMoving = false;
    }

    private string GetSaveKey()
    {
        return $"SlidingDoor_{doorID}";
    }

#if UNITY_EDITOR
    [ContextMenu("Reset Door Save")]
    private void ResetDoorSave()
    {
        PlayerPrefs.DeleteKey(GetSaveKey());
        PlayerPrefs.Save();

        isOpen = false;
        isMoving = false;

        transform.localPosition = closedPosition;

        Debug.Log($"SlidingDoor reset: {GetSaveKey()}");
    }
#endif

    private void OnValidate()
    {
        if (openDirection == Vector3.zero)
        {
            openDirection = Vector3.forward;
        }

        openDistance = Mathf.Max(0.01f, openDistance);
        openSpeed = Mathf.Max(0.01f, openSpeed);
    }
}

using UnityEngine;

[DisallowMultipleComponent]
public class DoubleDoor : MonoBehaviour
{
    [Header("Pivots")]
    [SerializeField] private Transform leftPivot;
    [SerializeField] private Transform rightPivot;

    [Header("Settings")]
    [SerializeField] private float openAngle = 70f;
    [SerializeField] private float speed = 4f;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private DoorLock doorLock;

    private bool isOpen;
    private bool isMoving;

    private float leftTarget;
    private float rightTarget;

    private void Awake()
    {
        doorLock = GetComponent<DoorLock>();
    }

    public void Interact()
    {
        if (isMoving)
            return;

        // =========================
        // LOCK SYSTEM
        // =========================
        if (doorLock != null)
        {
            GameObject playerObj = player.gameObject;

            bool canOpen = doorLock.TryUnlock(playerObj);

            if (!canOpen)
            {
                Debug.Log("[DOUBLE DOOR] Locked");
                return;
            }
        }

        isOpen = !isOpen;

        if (isOpen)
        {
            OpenAccordingToPlayer();
            PlaySound(openSound);
        }
        else
        {
            leftTarget = 0f;
            rightTarget = 0f;

            PlaySound(closeSound);
        }

        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving)
            return;

        RotateDoors();
    }

    private void OpenAccordingToPlayer()
    {
        Vector3 localPlayerPos =
            transform.InverseTransformPoint(player.position);

        bool playerIsFront = localPlayerPos.z > 0f;

        if (playerIsFront)
        {
            leftTarget = openAngle;
            rightTarget = -openAngle;
        }
        else
        {
            leftTarget = -openAngle;
            rightTarget = openAngle;
        }
    }

    private void RotateDoors()
    {
        RotatePivot(leftPivot, leftTarget);
        RotatePivot(rightPivot, rightTarget);

        bool leftDone =
            Mathf.Abs(
                Mathf.DeltaAngle(
                    leftPivot.localEulerAngles.y,
                    leftTarget
                )
            ) < 0.5f;

        bool rightDone =
            Mathf.Abs(
                Mathf.DeltaAngle(
                    rightPivot.localEulerAngles.y,
                    rightTarget
                )
            ) < 0.5f;

        if (leftDone && rightDone)
        {
            leftPivot.localEulerAngles =
                new Vector3(0f, leftTarget, 0f);

            rightPivot.localEulerAngles =
                new Vector3(0f, rightTarget, 0f);

            isMoving = false;
        }
    }

    private void RotatePivot(Transform pivot, float target)
    {
        float current = pivot.localEulerAngles.y;

        float next =
            Mathf.LerpAngle(
                current,
                target,
                Time.deltaTime * speed
            );

        pivot.localEulerAngles =
            new Vector3(0f, next, 0f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}

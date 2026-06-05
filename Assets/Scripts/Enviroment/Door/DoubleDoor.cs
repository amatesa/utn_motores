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
    [SerializeField] private AudioClip[] openSounds;
    [SerializeField] private AudioClip[] closeSounds;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomPitch = false;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private DoorLock doorLock;

    private bool isOpen;
    private bool isMoving;
    private int lastOpenSoundIndex = -1;
    private int lastCloseSoundIndex = -1;

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
            PlayClip(GetRandomClip(openSounds, ref lastOpenSoundIndex));
        }
        else
        {
            leftTarget = 0f;
            rightTarget = 0f;

            PlayClip(GetRandomClip(closeSounds, ref lastCloseSoundIndex));
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

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        float originalPitch = audioSource.pitch;
        if (randomPitch)
            audioSource.pitch = GetPlaybackPitch();

        audioSource.PlayOneShot(clip, volume);
        audioSource.pitch = originalPitch;
    }

    private float GetPlaybackPitch()
    {
        if (!randomPitch)
            return 1f;

        float min = Mathf.Min(pitchRange.x, pitchRange.y);
        float max = Mathf.Max(pitchRange.x, pitchRange.y);
        return Random.Range(min, max);
    }

    private AudioClip GetRandomClip(AudioClip[] clips, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0)
            return null;

        int index;
        if (clips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, clips.Length);
            }
            while (index == lastIndex);
        }

        lastIndex = index;
        return clips[index];
    }
}

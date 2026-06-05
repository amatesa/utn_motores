using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 4f;
    private DoorLock doorLock;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] openSounds;
    [SerializeField] private AudioClip[] closeSounds;
    [SerializeField] private AudioClip[] lockedSounds;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomPitch = false;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("AI")]
    [SerializeField] private bool enemyCanOpen = false;
    [SerializeField] private float autoCloseDelay = 10f;

    [Header("Interaction")]
    [SerializeField] private InteractionEmitter interactionEmitter;
    [SerializeField] private Transform player;

    private bool isOpen = false;
    private bool isMoving = false;
    private float targetAngle;
    private int lastOpenSoundIndex = -1;
    private int lastCloseSoundIndex = -1;
    private int lastLockedSoundIndex = -1;

    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        doorLock = GetComponent<DoorLock>();
    }
    private void Update()
    {
        if (isMoving)
        {
            RotateDoor();
        }
    }

    // =========================
    // PLAYER (compatibilidad)
    // =========================
    public void UseDoor()
    {
        UseDoor(null);
    }

    // =========================
    // NUEVO SISTEMA (con instigator)
    // =========================
    public void UseDoor(GameObject instigator)
    {
        if (isMoving) return;

        // =========================
        // LOCK SYSTEM (PLAYER)
        // =========================
        if (doorLock != null)
        {
            GameObject playerObj = player != null ? player.gameObject : instigator;
            Debug.Log("Instigator: " + (instigator != null ? instigator.name : "NULL"));
            if (playerObj != null && playerObj.CompareTag("Player"))
            {
                bool canOpen = doorLock.TryUnlock(playerObj);

                if (!canOpen)
                {
                    Debug.Log("[DOOR] Locked - cannot open");
                    PlayLockedSound();
                    return;
                }
            }
        }

        isOpen = !isOpen;

        float direction = GetOpenDirection(instigator);

        targetAngle = isOpen ? openAngle * direction : 0f;

        isMoving = true;

        if (CompareTag("ExitDoor") && isOpen)
        {
            //Debug.Log("[EXIT DOOR] Victory");

            GameManager.Instance.TriggerVictory();
        }


        if (isOpen)
            PlayClip(GetRandomClip(openSounds, ref lastOpenSoundIndex));
        else
            PlayClip(GetRandomClip(closeSounds, ref lastCloseSoundIndex));

        
        if (interactionEmitter != null)
        {
            if (instigator != null)
                interactionEmitter.Interact(instigator);
            else
                interactionEmitter.Interact();
        }

        
        if (instigator != null && instigator.CompareTag("Enemy") && isOpen)
        {
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());
        }
    }

    private IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (!isMoving && isOpen)
        {
            UseDoor(null); // cierre normal
        }
    }

    private void PlayLockedSound()
    {
        PlayClip(GetRandomClip(lockedSounds, ref lastLockedSoundIndex));
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null)
            return;

        if (audioSource != null)
        {
            float originalPitch = audioSource.pitch;
            if (randomPitch)
                audioSource.pitch = GetPlaybackPitch();

            audioSource.PlayOneShot(clip, volume);
            audioSource.pitch = originalPitch;
            return;
        }

        PlayClipAtPoint(clip);
    }

    private float GetPlaybackPitch()
    {
        if (!randomPitch)
            return 1f;

        float min = Mathf.Min(pitchRange.x, pitchRange.y);
        float max = Mathf.Max(pitchRange.x, pitchRange.y);
        return Random.Range(min, max);
    }

    private void PlayClipAtPoint(AudioClip clip)
    {
        if (!randomPitch)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
            return;
        }

        GameObject audioObject = new GameObject($"{name}_OneShotAudio");
        audioObject.transform.position = transform.position;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = GetPlaybackPitch();
        source.Play();

        Destroy(audioObject, clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)));
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

    private float GetOpenDirection(GameObject instigator)
    {
        Transform reference = player;

        if (instigator != null)
        {
            reference = instigator.transform;
        }

        Vector3 toAgent = reference.position - pivot.position;
        toAgent.y = 0;

        Vector3 right = pivot.right;

        float dot = Vector3.Dot(-right, toAgent);

        Debug.Log("[DOOR] DOT (" + reference.name + "): " + dot);

        return (dot > 0) ? 1f : -1f;
    }

    private void RotateDoor()
    {
        float currentY = pivot.localEulerAngles.y;

        float angle = Mathf.LerpAngle(currentY, targetAngle, Time.deltaTime * speed);
        pivot.localEulerAngles = new Vector3(0, angle, 0);

        if (Mathf.Abs(Mathf.DeltaAngle(currentY, targetAngle)) < 0.5f)
        {
            pivot.localEulerAngles = new Vector3(0, targetAngle, 0);
            isMoving = false;
        }
    }

    // =========================
    // USO PARA IA
    // =========================
    public void OpenDoorFromAI(Transform agent)
    {
        if (!enemyCanOpen) return;

        if (isMoving) return;

        UseDoor(agent.gameObject);
    }
}

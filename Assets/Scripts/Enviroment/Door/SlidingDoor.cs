using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DoorLock))]
public class SlidingDoor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 openDirection = Vector3.forward;
    [SerializeField] private float openDistance = 2f;
    [SerializeField] private float openSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] openSounds;
    [SerializeField] private AudioClip[] lockedSounds;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomPitch = false;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Persistence")]
    [SerializeField] private string doorID;

    private DoorLock doorLock;

    private bool isOpen;
    private bool isMoving;
    private int lastOpenSoundIndex = -1;
    private int lastLockedSoundIndex = -1;

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


    public void UseDoor(GameObject instigator)
    {
        if (isOpen || isMoving)
            return;

        if (doorLock != null && !doorLock.TryUnlock(instigator))
        {
            PlayLockedSound();
            return;
        }

        OpenDoor();
    }

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

        PlayOpenSound();

        PlayerPrefs.SetInt(GetSaveKey(), 1);
        PlayerPrefs.Save();

        StartCoroutine(OpenRoutine());
    }

    private void PlayOpenSound()
    {
        PlayClip(GetRandomClip(openSounds, ref lastOpenSoundIndex));
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

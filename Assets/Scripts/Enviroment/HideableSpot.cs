using System.Collections;
using UnityEngine;
using StarterAssets;

public class HideableSpot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPoint;

    [Header("Settings")]
    [SerializeField] private bool debug = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enterHideSound;
    [SerializeField] private AudioClip exitHideSound;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomPitch = false;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Whispers")]
    [SerializeField] private bool enableWhispers;
    [SerializeField] private AudioClip[] whisperClips;
    [SerializeField] private float whisperVolume = 1f;
    [SerializeField] private float minDelay = 3f;
    [SerializeField] private float maxDelay = 8f;

    [Header("Ghost Voices")]
    [SerializeField] private bool enableGhostVoices = true;
    [SerializeField] private AudioClip[] ghostVoiceClips;
    [SerializeField] private float ghostVoiceVolume = 1f;
    [SerializeField] private float minVoiceDelay = 2f;
    [SerializeField] private float maxVoiceDelay = 8f;
    [SerializeField] private int maxVoicePlaysPerHide = 1;
    [SerializeField] private bool allowVoiceRepeatInSameHide = false;

    private Coroutine ghostVoiceRoutine;

    private int voicePlaysThisHide;

    private readonly System.Collections.Generic.List<int> usedVoiceIndexes =
        new System.Collections.Generic.List<int>();

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
    private Coroutine whisperRoutine;
    private int lastWhisperIndex = -1;

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

        PlayClip(enterHideSound);
        StartWhispers();
        voicePlaysThisHide = 0;
        usedVoiceIndexes.Clear();

        StartGhostVoices();

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
        StopWhispers();
        StopGhostVoices();

        PlayClip(exitHideSound);

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
    private void StartGhostVoices()
    {
        StopGhostVoices();

        if (!enableGhostVoices)
            return;

        if (ghostVoiceClips == null || ghostVoiceClips.Length == 0)
            return;

        ghostVoiceRoutine = StartCoroutine(GhostVoiceLoop());
    }

    private void StopGhostVoices()
    {
        if (ghostVoiceRoutine == null)
            return;

        StopCoroutine(ghostVoiceRoutine);
        ghostVoiceRoutine = null;
    }

    private IEnumerator GhostVoiceLoop()
    {
        while (isHiding)
        {
            if (voicePlaysThisHide >= maxVoicePlaysPerHide)
                break;

            float min = Mathf.Max(0f, minVoiceDelay);
            float max = Mathf.Max(min, maxVoiceDelay);

            yield return new WaitForSeconds(
                Random.Range(min, max)
            );

            if (!isHiding)
                continue;

            AudioClip clip = GetRandomGhostVoice();

            if (clip == null)
                continue;

            PlayGhostVoiceClip(clip);

            voicePlaysThisHide++;
        }

        ghostVoiceRoutine = null;
    }

    private AudioClip GetRandomGhostVoice()
    {
        if (ghostVoiceClips == null || ghostVoiceClips.Length == 0)
            return null;

        if (!allowVoiceRepeatInSameHide &&
            usedVoiceIndexes.Count >= ghostVoiceClips.Length)
            return null;

        int index;

        do
        {
            index = Random.Range(0, ghostVoiceClips.Length);
        }
        while (
            !allowVoiceRepeatInSameHide &&
            usedVoiceIndexes.Contains(index)
        );

        if (!allowVoiceRepeatInSameHide)
            usedVoiceIndexes.Add(index);

        return ghostVoiceClips[index];
    }

    private void PlayGhostVoiceClip(AudioClip clip)
    {
        if (audioSource != null)
        {
            float originalPitch = audioSource.pitch;

            audioSource.pitch = 1f;

            audioSource.PlayOneShot(
                clip,
                volume * ghostVoiceVolume
            );

            audioSource.pitch = originalPitch;

            return;
        }

        AudioSource.PlayClipAtPoint(
            clip,
            transform.position,
            volume * ghostVoiceVolume
        );
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

        PlayClipAtPoint(clip, volume);
    }

    private float GetPlaybackPitch()
    {
        if (!randomPitch)
            return 1f;

        float min = Mathf.Min(pitchRange.x, pitchRange.y);
        float max = Mathf.Max(pitchRange.x, pitchRange.y);
        return Random.Range(min, max);
    }

    private void StartWhispers()
    {
        StopWhispers();

        if (!enableWhispers || whisperClips == null || whisperClips.Length == 0)
            return;

        whisperRoutine = StartCoroutine(WhisperLoop());
    }

    private void StopWhispers()
    {
        if (whisperRoutine == null)
            return;

        StopCoroutine(whisperRoutine);
        whisperRoutine = null;
    }

    private IEnumerator WhisperLoop()
    {
        while (isHiding)
        {
            float min = Mathf.Max(0f, minDelay);
            float max = Mathf.Max(min, maxDelay);
            yield return new WaitForSeconds(Random.Range(min, max));

            if (!isHiding || !enableWhispers)
                continue;

            AudioClip clip = GetRandomWhisperClip();
            if (clip == null)
                continue;

            PlayWhisperClip(clip);
        }

        whisperRoutine = null;
    }

    private AudioClip GetRandomWhisperClip()
    {
        if (whisperClips == null || whisperClips.Length == 0)
            return null;

        int index = Random.Range(0, whisperClips.Length);

        if (whisperClips.Length > 1)
        {
            while (index == lastWhisperIndex)
            {
                index = Random.Range(0, whisperClips.Length);
            }
        }

        lastWhisperIndex = index;
        return whisperClips[index];
    }

    private void PlayWhisperClip(AudioClip clip)
    {
        if (audioSource != null)
        {
            float originalPitch = audioSource.pitch;
            if (randomPitch)
                audioSource.pitch = GetPlaybackPitch();

            audioSource.PlayOneShot(clip, volume * whisperVolume);
            audioSource.pitch = originalPitch;
            return;
        }

        PlayClipAtPoint(clip, volume * whisperVolume);
    }

    private void PlayClipAtPoint(AudioClip clip, float clipVolume)
    {
        if (!randomPitch)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, clipVolume);
            return;
        }

        GameObject audioObject = new GameObject($"{name}_OneShotAudio");
        audioObject.transform.position = transform.position;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = clipVolume;
        source.pitch = GetPlaybackPitch();
        source.Play();

        Destroy(audioObject, clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)));
    }

    public void ToggleHide(GameObject player)
    {
        if (isHiding)
            ExitHide();
        else
            EnterHide(player);
    }
}

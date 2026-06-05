using System.Collections;
using UnityEngine;

public class GhostTriggerEvent : MonoBehaviour
{
    [Header("Ghost")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private bool usePlayerAsTarget;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private float activationDelay;

    [Header("Optional Audio Trigger")]
    [SerializeField] private AudioSource optionalAudioSource;
    [SerializeField] private AudioClip optionalAudioClip;

    [Header("Optional Noise Event")]
    [SerializeField] private bool emitNoiseEvent;
    [SerializeField] private float noiseIntensity = 10f;

    private bool hasTriggered;
    private Coroutine pendingTriggerRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        TriggerEvent();
    }

    public void TriggerEvent()
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        if (pendingTriggerRoutine != null)
            StopCoroutine(pendingTriggerRoutine);

        if (activationDelay <= 0f)
        {
            ActivateEvent();
            return;
        }

        pendingTriggerRoutine = StartCoroutine(DelayedActivationRoutine());
    }

    private IEnumerator DelayedActivationRoutine()
    {
        yield return new WaitForSeconds(activationDelay);
        ActivateEvent();
        pendingTriggerRoutine = null;
    }

    private void ActivateEvent()
    {
        Vector3 spawnPosition = pointA != null ? pointA.position : transform.position;
        Quaternion spawnRotation = pointA != null ? pointA.rotation : transform.rotation;

        GameObject ghost = SpawnGhost(spawnPosition, spawnRotation);

        PlayOptionalAudio(spawnPosition);
        EmitOptionalNoise(spawnPosition);
        ConfigureGhostMovement(ghost);
    }

    private GameObject SpawnGhost(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (ghostPrefab == null)
        {
            Debug.LogWarning($"[GhostTriggerEvent] Missing ghost prefab on {name}.");
            return null;
        }

        return Instantiate(ghostPrefab, spawnPosition, spawnRotation);
    }

    private void ConfigureGhostMovement(GameObject ghost)
    {
        if (ghost == null)
            return;

        GhostMovement movement = ghost.GetComponent<GhostMovement>();
        if (movement == null)
            movement = ghost.AddComponent<GhostMovement>();

        if (usePlayerAsTarget)
        {
            Transform playerTarget = GetPlayerTarget();
            if (playerTarget != null)
            {
                movement.Begin(playerTarget);
                return;
            }
        }

        Vector3 destination = pointB != null ? pointB.position : ghost.transform.position;
        movement.Begin(destination);
    }

    private Transform GetPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    private void PlayOptionalAudio(Vector3 position)
    {
        if (optionalAudioClip == null)
            return;

        if (optionalAudioSource != null)
        {
            optionalAudioSource.PlayOneShot(optionalAudioClip);
            return;
        }

        AudioSource.PlayClipAtPoint(optionalAudioClip, position);
    }

    private void EmitOptionalNoise(Vector3 position)
    {
        if (!emitNoiseEvent || NoiseSystem.Instance == null)
            return;

        NoiseSystem.Instance.EmitSound(
            position,
            noiseIntensity,
            SoundEmitterType.Environment,
            gameObject
        );
    }

}

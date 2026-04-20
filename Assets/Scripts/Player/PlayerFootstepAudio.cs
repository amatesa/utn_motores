using UnityEngine;
using System.Collections.Generic;
using StarterAssets;

/// <summary>
/// RESPONSABILIDAD:
/// Controlar el audio de pasos del jugador basado en:
/// - velocidad (walk / run)
/// - superficie (tag del suelo)
///
/// NO interactúa con IA ni NoiseSystem.
/// Solo feedback auditivo.
///
/// DISEÑO:
/// Sistema basado en intervalos (step timing),
/// evitando loops para lograr pasos más naturales.
/// </summary>

[System.Serializable]
public class SurfaceType
{
    public string surfaceTag;
    public AudioClip[] footstepSounds;
}

public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Surfaces")]
    [SerializeField] private List<SurfaceType> surfaceTypes;

    [Header("Step Timing")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.35f;

    [Header("Detection")]
    [SerializeField] private float rayDistance = 1.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Volumen")]
    [SerializeField] private float footstepVolume = 0.3f;

    private CharacterController controller;
    private StarterAssetsInputs input;

    private float stepTimer;
    private int lastPlayedIndex = -1;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<StarterAssetsInputs>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        if (PlayerSafeState.Instance.IsSafe)
            return;

        float speed = controller.velocity.magnitude;

        if (speed < 0.1f)
            return;

        if (input.stealth)
            return;

        stepTimer += Time.deltaTime;

        float interval = input.sprint ? runStepInterval : walkStepInterval;

        if (stepTimer >= interval)
        {
            PlayFootstep();
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        AudioClip[] clips = GetSurfaceClips();

        if (clips == null || clips.Length == 0)
            return;

        int index;

        do
        {
            index = Random.Range(0, clips.Length);
        }
        while (index == lastPlayedIndex);

        audioSource.PlayOneShot(clips[index]);

        lastPlayedIndex = index;
    }

    AudioClip[] GetSurfaceClips()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, groundLayer))
        {
            string tag = hit.collider.tag;

            foreach (var surface in surfaceTypes)
            {
                if (surface.surfaceTag == tag)
                    return surface.footstepSounds;
            }
        }

        return null;
    }
}

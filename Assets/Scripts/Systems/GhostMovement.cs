using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private bool destroyAfterCompletion = true;

    [Header("Ghost Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ghostAudioClip;
    [SerializeField] private bool playAudioOnSpawn = true;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 20f;

    private const float ArrivalDistance = 0.1f;

    private readonly List<MaterialFadeData> fadeMaterials = new List<MaterialFadeData>();
    private Coroutine movementRoutine;
    private Transform targetTransform;
    private Vector3 targetPosition;
    private bool useTargetTransform;

    public void Begin(Vector3 destination)
    {
        targetPosition = destination;
        targetTransform = null;
        useTargetTransform = false;
        StartMovement();
    }

    public void Begin(Transform destination)
    {
        targetTransform = destination;
        targetPosition = destination != null ? destination.position : transform.position;
        useTargetTransform = destination != null;
        StartMovement();
    }

    private void StartMovement()
    {
        if (movementRoutine != null)
            StopCoroutine(movementRoutine);

        CacheFadeMaterials();
        ConfigureAudio();
        movementRoutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        if (playAudioOnSpawn && audioSource != null && audioSource.clip != null)
            audioSource.Play();

        yield return FadeRoutine(0f, 1f, fadeInTime);
        yield return MoveToTargetRoutine();
        yield return FadeRoutine(1f, 0f, fadeOutTime);

        if (destroyAfterCompletion)
            Destroy(gameObject);
    }

    private IEnumerator MoveToTargetRoutine()
    {
        if (maxSpeed <= 0f)
            yield break;

        Vector3 startPosition = transform.position;
        float initialDistance = Vector3.Distance(startPosition, GetTargetPosition());

        while (Vector3.Distance(transform.position, GetTargetPosition()) > ArrivalDistance)
        {
            Vector3 destination = GetTargetPosition();
            Vector3 direction = destination - transform.position;

            if (lookAtTarget && direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }

            float remainingDistance = Vector3.Distance(transform.position, destination);
            float progress = initialDistance <= 0f ? 1f : 1f - Mathf.Clamp01(remainingDistance / initialDistance);
            float speedMultiplier = speedCurve != null ? Mathf.Max(0.01f, speedCurve.Evaluate(progress)) : 1f;
            float step = Mathf.Max(0f, maxSpeed) * speedMultiplier * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, destination, step);
            yield return null;
        }
    }

    private Vector3 GetTargetPosition()
    {
        if (useTargetTransform && targetTransform != null)
            return targetTransform.position;

        return targetPosition;
    }

    private void ConfigureAudio()
    {
        if (ghostAudioClip == null)
            return;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = ghostAudioClip;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
    }

    private void CacheFadeMaterials()
    {
        fadeMaterials.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rendererRef in renderers)
        {
            foreach (Material material in rendererRef.materials)
            {
                if (material == null)
                    continue;

                if (material.HasProperty("_BaseColor"))
                {
                    fadeMaterials.Add(new MaterialFadeData(material, "_BaseColor", material.GetColor("_BaseColor")));
                }
                else if (material.HasProperty("_Color"))
                {
                    fadeMaterials.Add(new MaterialFadeData(material, "_Color", material.GetColor("_Color")));
                }
            }
        }
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        if (fadeMaterials.Count == 0)
            yield break;

        if (duration <= 0f)
        {
            SetAlpha(toAlpha);
            yield break;
        }

        float elapsed = 0f;
        SetAlpha(fromAlpha);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration));
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(toAlpha);
    }

    private void SetAlpha(float alpha)
    {
        foreach (MaterialFadeData data in fadeMaterials)
        {
            Color color = data.OriginalColor;
            color.a = alpha;
            data.Material.SetColor(data.ColorProperty, color);
        }
    }

    private class MaterialFadeData
    {
        public readonly Material Material;
        public readonly string ColorProperty;
        public readonly Color OriginalColor;

        public MaterialFadeData(Material material, string colorProperty, Color originalColor)
        {
            Material = material;
            ColorProperty = colorProperty;
            OriginalColor = originalColor;
        }
    }
}

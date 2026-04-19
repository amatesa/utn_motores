using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Controla la visibilidad del mesh del player al cambiar entre 1ra y 3ra persona.
/// Solo afecta renderizado, no colliders ni layers (no rompe percepción del enemigo).
/// </summary>
public class FirstPersonBodyVisibilityController : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Si está vacío, intentará encontrar automáticamente el root del Player.")]
    [SerializeField] private Transform meshRoot;
    [SerializeField] private List<Renderer> explicitRenderers = new();
    [SerializeField] private List<Renderer> excludeRenderers = new();

    [Header("First Person")]
    [SerializeField] private bool useShadowsOnlyInFirstPerson = true;

    private readonly List<Renderer> _managedRenderers = new();
    private readonly Dictionary<Renderer, ShadowCastingMode> _originalShadowModes = new();

    private void Awake()
    {
        CacheRenderers();
    }

    public void SetFirstPersonMode(bool isFirstPerson)
    {
        if (_managedRenderers.Count == 0)
        {
            CacheRenderers();
        }

        foreach (Renderer rendererRef in _managedRenderers)
        {
            if (rendererRef == null) continue;

            if (isFirstPerson)
            {
                if (useShadowsOnlyInFirstPerson)
                {
                    rendererRef.enabled = true;
                    rendererRef.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }
                else
                {
                    rendererRef.enabled = false;
                }
            }
            else
            {
                rendererRef.enabled = true;
                if (_originalShadowModes.TryGetValue(rendererRef, out ShadowCastingMode originalMode))
                {
                    rendererRef.shadowCastingMode = originalMode;
                }
            }
        }
    }

    private void CacheRenderers()
    {
        _managedRenderers.Clear();
        _originalShadowModes.Clear();

        if (explicitRenderers.Count > 0)
        {
            foreach (Renderer rendererRef in explicitRenderers)
            {
                AddRendererIfValid(rendererRef);
            }
            return;
        }

        Transform root = ResolveMeshRoot();
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rendererRef in renderers)
        {
            AddRendererIfValid(rendererRef);
        }
    }

    private void AddRendererIfValid(Renderer rendererRef)
    {
        if (rendererRef == null) return;
        if (excludeRenderers.Contains(rendererRef)) return;
        if (_managedRenderers.Contains(rendererRef)) return;

        _managedRenderers.Add(rendererRef);
        _originalShadowModes[rendererRef] = rendererRef.shadowCastingMode;
    }

    private Transform ResolveMeshRoot()
    {
        if (meshRoot != null)
        {
            return meshRoot;
        }

        CharacterController controllerInParents = GetComponentInParent<CharacterController>();
        if (controllerInParents != null)
        {
            return controllerInParents.transform;
        }

        StarterAssets.StarterAssetsInputs playerInputs = FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
        if (playerInputs != null)
        {
            return playerInputs.transform;
        }

        return transform;
    }
}

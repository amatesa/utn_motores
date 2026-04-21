using UnityEngine;
using System.Collections.Generic;

public class RoofOcclusionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Camera cam;

    [Header("Settings")]
    [SerializeField] private LayerMask roofLayer;
    [SerializeField] private float fadeAlpha = 0.2f;
    [SerializeField] private float fadeSpeed = 5f;

    private List<Renderer> currentHidden = new List<Renderer>();
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    void LateUpdate()
    {
        RestoreObjects();

        Vector3 direction = target.position - cam.transform.position;
        float distance = direction.magnitude;

        Ray ray = new Ray(cam.transform.position, direction.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, roofLayer);

        foreach (RaycastHit hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                HideObject(rend);
            }
        }
    }

    void HideObject(Renderer rend)
    {
        if (!originalColors.ContainsKey(rend))
        {
            originalColors[rend] = rend.material.GetColor("_BaseColor");
        }

        Color currentColor = rend.material.GetColor("_BaseColor");
        currentColor.a = Mathf.Lerp(currentColor.a, fadeAlpha, Time.deltaTime * fadeSpeed);
        rend.material.SetColor("_BaseColor", currentColor);

        if (!currentHidden.Contains(rend))
            currentHidden.Add(rend);
    }

    void RestoreObjects()
    {
        foreach (Renderer rend in currentHidden)
        {
            if (rend != null && originalColors.ContainsKey(rend))
            {
                Color currentColor = rend.material.GetColor("_BaseColor");
                currentColor.a = Mathf.Lerp(currentColor.a, 1f, Time.deltaTime * fadeSpeed);
                rend.material.SetColor("_BaseColor", currentColor);
            }
        }

        currentHidden.Clear();
    }
}

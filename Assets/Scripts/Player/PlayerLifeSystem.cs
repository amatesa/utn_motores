using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerLifeSystem : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 4;
    [SerializeField] private float hitCooldown = 1.2f;
    [SerializeField] private float postRecoverImmunity = 0.6f;

    [Header("Transitions")]
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float blackoutDuration = 0.55f;
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private bool fadePlayerMesh = false;

    [Header("Screen Blackout (UI Overlay)")]
    [Tooltip("Optional. If null, the script auto-creates a fullscreen overlay image at runtime.")]
    [SerializeField] private Image screenFadeImage;
    [SerializeField] private Color blackoutColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private float blackoutAlpha = 0.92f;

    [Header("Color Degradation (URP Volume)")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float minSaturation = -90f;
    [SerializeField] private float minContrast = -40f;
    [SerializeField] private float minPostExposure = -1.25f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    [Header("Game Over")]
    [SerializeField] private UnityEvent onGameOver;

    public int CurrentLives { get; private set; }
    public bool IsGameOver => CurrentLives <= 0;
    public float CurrentLifeNormalized => (float)CurrentLives / maxLives;

    private StarterAssets.StarterAssetsInputs starterInputs;
    private StarterAssets.ThirdPersonController thirdPersonController;
    private StarterAssets.FirstPersonController firstPersonController;

    private Renderer[] cachedRenderers;
    private float hitCooldownTimer;
    private bool transitioning;
    
    private ColorAdjustments colorAdjustments;

    private void Awake()
    {
        CurrentLives = maxLives;

        starterInputs = GetComponent<StarterAssets.StarterAssetsInputs>();
        thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
        firstPersonController = GetComponent<StarterAssets.FirstPersonController>();

        cachedRenderers = GetComponentsInChildren<Renderer>(true);

        if (postProcessVolume == null)
        {
            postProcessVolume = FindAnyObjectByType<Volume>();
            if (postProcessVolume != null)
                DebugLog("[PLAYER LIVES] Auto-found Volume in scene.");
        }

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out colorAdjustments);
        }

        EnsureScreenOverlay();
        SetOverlayAlpha(0f);

        DebugLog($"[PLAYER LIVES] Init maxLives={maxLives}, currentLives={CurrentLives}");
    }

    private void Update()
    {
        if (hitCooldownTimer > 0f)
        {
            hitCooldownTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHandleEnemyContact(other.gameObject, "OnTriggerEnter");
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHandleEnemyContact(collision.gameObject, "OnCollisionEnter");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        TryHandleEnemyContact(hit.gameObject, "OnControllerColliderHit");
    }

    private void TryHandleEnemyContact(GameObject contactedObject, string source)
    {
        if (IsGameOver)
        {
            DebugLog($"[PLAYER LIVES] Ignored hit from {source}: game already over.");
            return;
        }

        if (transitioning)
        {
            DebugLog($"[PLAYER LIVES] Ignored hit from {source}: transition in progress.");
            return;
        }

        if (hitCooldownTimer > 0f)
        {
            DebugLog($"[PLAYER LIVES] Ignored hit from {source}: cooldown {hitCooldownTimer:F2}s left.");
            return;
        }

        ShadowEnemyTeleportController enemyTeleport = contactedObject.GetComponentInParent<ShadowEnemyTeleportController>();
        bool isEnemyTag = contactedObject.CompareTag("Enemy");

        if (enemyTeleport == null && !isEnemyTag)
            return;

        DebugLog($"[PLAYER LIVES] Enemy contact detected from {source} ({contactedObject.name}). Lives before hit: {CurrentLives}");
        StartCoroutine(LoseLifeRoutine(enemyTeleport));
    }

    private IEnumerator LoseLifeRoutine(ShadowEnemyTeleportController enemyTeleport)
    {
        transitioning = true;

        float fullTransitionDuration = fadeOutDuration + blackoutDuration + fadeInDuration;
        hitCooldownTimer = Mathf.Max(hitCooldown, fullTransitionDuration + postRecoverImmunity);

        SetPlayerControlEnabled(false);

        // Teleport inmediato para romper colisión física al inicio del golpe.
        if (enemyTeleport != null)
        {
            bool firstTeleport = enemyTeleport.TeleportAfterPlayerHit(transform, enemyTeleport.gameObject, true);
            DebugLog($"[PLAYER LIVES] First hit teleport => {(firstTeleport ? "SUCCESS" : "FAILED")}");
        }
        else
        {
            DebugLogWarning("[PLAYER LIVES] Enemy collision detected but no ShadowEnemyTeleportController was found.");
        }

        if (fadePlayerMesh)
            yield return FadePlayer(1f, 0f, fadeOutDuration);

        yield return FadeScreen(0f, blackoutAlpha, fadeOutDuration);
        yield return new WaitForSeconds(blackoutDuration);

        CurrentLives = Mathf.Max(0, CurrentLives - 1);
        DebugLog($"[PLAYER LIVES] Life lost. Remaining={CurrentLives}");

        ApplyColorDegradation();

        if (CurrentLives <= 0)
        {
            onGameOver?.Invoke();
            DebugLogWarning("[PLAYER LIVES] GAME OVER triggered.");
            yield break;
        }

        // Teleport extra antes de devolver control, para evitar spawn cercano tras blackout.
        if (enemyTeleport != null)
        {
            bool safetyTeleport = enemyTeleport.TeleportAfterPlayerHit(transform, enemyTeleport.gameObject, true);
            DebugLog($"[PLAYER LIVES] Safety teleport before wake-up => {(safetyTeleport ? "SUCCESS" : "FAILED")}");
        }

        if (fadePlayerMesh)
            yield return FadePlayer(0f, 1f, fadeInDuration);

        yield return FadeScreen(blackoutAlpha, 0f, fadeInDuration);

        SetPlayerControlEnabled(true);
        transitioning = false;

        DebugLog($"[PLAYER LIVES] Recovery complete. Player controls enabled. Lives={CurrentLives}");
    }

    private IEnumerator FadeScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (screenFadeImage == null)
        {
            DebugLogWarning("[PLAYER LIVES] No screenFadeImage assigned/created. Screen fade skipped.");
            yield break;
        }

        if (duration <= 0f)
        {
            SetOverlayAlpha(toAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            SetOverlayAlpha(alpha);
            yield return null;
        }

        SetOverlayAlpha(toAlpha);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (screenFadeImage == null)
            return;

        Color c = blackoutColor;
        c.a = Mathf.Clamp01(alpha);
        screenFadeImage.color = c;
    }

    private void EnsureScreenOverlay()
    {
        if (screenFadeImage != null)
            return;

        GameObject canvasGO = new GameObject("LifeSystemScreenFadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject imageGO = new GameObject("LifeSystemScreenFadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        screenFadeImage = imageGO.AddComponent<Image>();

        RectTransform rt = screenFadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        screenFadeImage.raycastTarget = false;

        DebugLog("[PLAYER LIVES] Auto-created runtime screen fade overlay.");
    }

    private IEnumerator FadePlayer(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            ApplyRenderAlpha(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(from, to, t);
            ApplyRenderAlpha(alpha);
            yield return null;
        }

        ApplyRenderAlpha(to);
    }

    private void ApplyRenderAlpha(float alpha)
    {
        foreach (Renderer rendererRef in cachedRenderers)
        {
            if (rendererRef == null) continue;

            Material[] mats = rendererRef.materials;
            foreach (Material mat in mats)
            {
                if (mat == null) continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
    }

    private void ApplyColorDegradation()
    {
        if (colorAdjustments == null)
        {
            DebugLogWarning("[PLAYER LIVES] No ColorAdjustments override found in assigned/scene Volume. Color degradation skipped.");
            return;
        }

        int livesLost = maxLives - CurrentLives;
        float normalizedLoss = Mathf.Clamp01((float)livesLost / maxLives);

        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.contrast.overrideState = true;
        colorAdjustments.postExposure.overrideState = true;

        colorAdjustments.saturation.value = Mathf.Lerp(0f, minSaturation, normalizedLoss);
        colorAdjustments.contrast.value = Mathf.Lerp(0f, minContrast, normalizedLoss);
        colorAdjustments.postExposure.value = Mathf.Lerp(0f, minPostExposure, normalizedLoss);

        DebugLog($"[PLAYER LIVES] PostFX updated. livesLost={livesLost}, saturation={colorAdjustments.saturation.value:F1}, contrast={colorAdjustments.contrast.value:F1}, exposure={colorAdjustments.postExposure.value:F2}");
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (starterInputs != null)
        {
            starterInputs.enabled = enabled;
            if (!enabled)
            {
                starterInputs.move = Vector2.zero;
                starterInputs.look = Vector2.zero;
                starterInputs.jump = false;
                starterInputs.sprint = false;
                starterInputs.stealth = false;
                starterInputs.switchCamera = false;
            }
        }

        if (thirdPersonController != null)
            thirdPersonController.enabled = enabled;

        if (firstPersonController != null)
            firstPersonController.enabled = enabled;

        DebugLog($"[PLAYER LIVES] Player controls {(enabled ? "ENABLED" : "DISABLED")}");
    }

    private void DebugLog(string message)
    {
        if (!debugEnabled) return;
        Debug.Log(message);
    }

    private void DebugLogWarning(string message)
    {
        if (!debugEnabled) return;
        Debug.LogWarning(message);
    }
}

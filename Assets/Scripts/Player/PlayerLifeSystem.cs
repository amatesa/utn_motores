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
        //VIDA PERSISTENTE
        if (GameManager.Instance != null)
        {
            maxLives = GameManager.Instance.MaxPlayerLives;

            if (GameManager.Instance.PlayerLives <= 0)
                GameManager.Instance.PlayerLives = maxLives;

            CurrentLives = GameManager.Instance.PlayerLives;
        }
        else
        {
            CurrentLives = maxLives;
        }

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

        DebugLog($"[PLAYER LIVES] Init currentLives={CurrentLives}");
    }

    private void Update()
    {
        if (hitCooldownTimer > 0f)
            hitCooldownTimer -= Time.deltaTime;
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
        if (IsGameOver || transitioning || hitCooldownTimer > 0f)
            return;

        ShadowEnemyTeleportController enemyTeleport = contactedObject.GetComponentInParent<ShadowEnemyTeleportController>();
        bool isEnemyTag = contactedObject.CompareTag("Enemy");

        if (enemyTeleport == null && !isEnemyTag)
            return;

        DebugLog($"[PLAYER LIVES] Enemy contact detected from {source}");

        StartCoroutine(LoseLifeRoutine(enemyTeleport));
    }

    private IEnumerator LoseLifeRoutine(ShadowEnemyTeleportController enemyTeleport)
    {
        transitioning = true;

        float fullTransitionDuration = fadeOutDuration + blackoutDuration + fadeInDuration;
        hitCooldownTimer = Mathf.Max(hitCooldown, fullTransitionDuration + postRecoverImmunity);

        SetPlayerControlEnabled(false);

        //TELEPORT INICIAL
        if (enemyTeleport != null)
        {
            enemyTeleport.TeleportAfterPlayerHit(transform, enemyTeleport.gameObject, true);
        }

        if (fadePlayerMesh)
            yield return FadePlayer(1f, 0f, fadeOutDuration);

        yield return FadeScreen(0f, blackoutAlpha, fadeOutDuration);
        yield return new WaitForSeconds(blackoutDuration);

        //VIDA PERSISTENTE
        CurrentLives = Mathf.Max(0, CurrentLives - 1);

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerLives = CurrentLives;

        ApplyColorDegradation();

        if (CurrentLives <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
            else
            {
                Debug.LogError("[PLAYER LIVES] GameManager not found");
            }

            yield break;
        }

        //TELEPORT DE SEGURIDAD
        if (enemyTeleport != null)
        {
            enemyTeleport.TeleportAfterPlayerHit(transform, enemyTeleport.gameObject, true);
        }

        if (fadePlayerMesh)
            yield return FadePlayer(0f, 1f, fadeInDuration);

        yield return FadeScreen(blackoutAlpha, 0f, fadeInDuration);

        SetPlayerControlEnabled(true);
        transitioning = false;
    }

    private IEnumerator FadeScreen(float fromAlpha, float toAlpha, float duration)
    {
        if (screenFadeImage == null)
            yield break;

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

            foreach (Material mat in rendererRef.materials)
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
            return;

        int livesLost = maxLives - CurrentLives;
        float normalizedLoss = Mathf.Clamp01((float)livesLost / maxLives);

        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.contrast.overrideState = true;
        colorAdjustments.postExposure.overrideState = true;

        colorAdjustments.saturation.value = Mathf.Lerp(0f, minSaturation, normalizedLoss);
        colorAdjustments.contrast.value = Mathf.Lerp(0f, minContrast, normalizedLoss);
        colorAdjustments.postExposure.value = Mathf.Lerp(0f, minPostExposure, normalizedLoss);
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
    }

    private void DebugLog(string message)
    {
        if (debugEnabled)
            Debug.Log(message);
    }
}

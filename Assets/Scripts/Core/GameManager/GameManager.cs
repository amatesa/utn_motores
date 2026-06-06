using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    //[SerializeField] private string testScene = "TestScene";
    [SerializeField] private string firstLevelScene = "Level_1";
    [SerializeField] private string secondLevelScene = "Level_2";
    [SerializeField] private string thirdLevelScene = "Level_3";
    [SerializeField] private string victoryOverScene = "VictoryScene";
    [SerializeField] private string gameOverScene = "GameOverScene";

    [Header("Player Data")]
    public int PlayerLives;
    public int MaxPlayerLives = 4;

    [Header("Loading Transition")]
    [SerializeField] private bool useLoadingTransition = true;
    [SerializeField] private float fadeToBlackDuration = 0.2f;
    [SerializeField] private float fadeFromBlackDuration = 0.35f;
    [SerializeField] private float minimumBlackScreenTime = 0.1f;
    [SerializeField] private int loadingOverlaySortingOrder = 32767;

    private bool hasSceneTransition = false;
    private bool isLoadingScene;
    private Coroutine sceneLoadRoutine;
    private CanvasGroup loadingOverlay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializePlayerData();
        EnsureLoadingOverlay();
    }

    private void InitializePlayerData()
    {
        if (PlayerLives <= 0)
        {
            PlayerLives = MaxPlayerLives;
        }
    }

    // =========================
    // LEVEL SYSTEM
    // =========================

    public void StartGame()
    {
        Time.timeScale = 1f;
        BeginLoadScene(firstLevelScene, false);
    }

    public void LoadLevel(string sceneName, string spawnID)
    {
        LevelSpawnManager.SetNextSpawn(spawnID);
        Debug.Log("SET SPAWN: " + spawnID);

        BeginLoadScene(sceneName, true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ThoughtPopupSystem.Instance?.ClearImmediate();

        if (!hasSceneTransition || isLoadingScene)
            return;

        StartCoroutine(RevealAfterLegacySceneLoad());

        hasSceneTransition = false;
    }

    private void RepositionPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[GameManager] Player not found");
            return;
        }

        string targetSpawn = LevelSpawnManager.NextSpawnID;

        Debug.Log("[GameManager] Spawn buscado: " + targetSpawn);

        LevelSpawnPoint[] points = FindObjectsByType<LevelSpawnPoint>(FindObjectsSortMode.None);

        foreach (var point in points)
        {
            if (point.SpawnID == targetSpawn)
            {
                Debug.Log("[GameManager] Spawn encontrado → mover antes del render");

                player.transform.SetPositionAndRotation(
                    point.transform.position,
                    point.transform.rotation
                );

                return;
            }
        }

        Debug.LogWarning("[GameManager] Spawn no encontrado: " + targetSpawn);
    }

    // =========================
    // GAME FLOW
    // =========================

    public void TriggerGameOver()
    {
        Time.timeScale = 0f;
        BeginLoadScene(gameOverScene, false);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        BeginLoadScene(mainMenuScene, false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TriggerVictory()
    {
        Time.timeScale = 1f;
        BeginLoadScene(victoryOverScene, false);
    }

    private void BeginLoadScene(string sceneName, bool repositionPlayer)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        if (isLoadingScene)
            return;

        if (sceneLoadRoutine != null)
            StopCoroutine(sceneLoadRoutine);

        sceneLoadRoutine = StartCoroutine(LoadSceneRoutine(sceneName, repositionPlayer));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, bool repositionPlayer)
    {
        isLoadingScene = true;
        hasSceneTransition = false;

        ThoughtPopupSystem.Instance?.ClearImmediate();

        if (useLoadingTransition)
        {
            EnsureLoadingOverlay();
            yield return FadeLoadingOverlay(1f, fadeToBlackDuration);
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            isLoadingScene = false;
            sceneLoadRoutine = null;
            yield break;
        }

        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
            yield return null;

        operation.allowSceneActivation = true;

        while (!operation.isDone)
            yield return null;

        ThoughtPopupSystem.Instance?.ClearImmediate();

        if (repositionPlayer)
            yield return FinalReposition();
        else
            yield return null;

        if (useLoadingTransition)
        {
            if (minimumBlackScreenTime > 0f)
                yield return new WaitForSecondsRealtime(minimumBlackScreenTime);

            yield return FadeLoadingOverlay(0f, fadeFromBlackDuration);
        }

        isLoadingScene = false;
        sceneLoadRoutine = null;
    }

    private IEnumerator RevealAfterLegacySceneLoad()
    {
        if (useLoadingTransition)
        {
            EnsureLoadingOverlay();
            SetLoadingOverlayAlpha(1f);
        }

        yield return FinalReposition();

        if (useLoadingTransition)
            yield return FadeLoadingOverlay(0f, fadeFromBlackDuration);
    }

    private IEnumerator FinalReposition()
    {
        GameObject player = null;

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        var controller = player.GetComponent<CharacterController>();

        // Desactivar controller
        if (controller != null)
            controller.enabled = false;

        // Esperar 1 frame limpio
        yield return null;

        string targetSpawn = LevelSpawnManager.NextSpawnID;

        LevelSpawnPoint[] points = FindObjectsByType<LevelSpawnPoint>(FindObjectsSortMode.None);

        foreach (var point in points)
        {
            if (point.SpawnID == targetSpawn)
            {
                player.transform.position = point.transform.position;
                player.transform.rotation = point.transform.rotation;
                break;
            }
        }

        // Resetear velocidad (CLAVE)
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.zero;

        // Reactivar controller
        if (controller != null)
            controller.enabled = true;
    }

    private void EnsureLoadingOverlay()
    {
        if (loadingOverlay != null)
            return;

        GameObject overlayObject = new GameObject("LoadingTransitionOverlay");
        DontDestroyOnLoad(overlayObject);

        Canvas canvas = overlayObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = loadingOverlaySortingOrder;

        loadingOverlay = overlayObject.AddComponent<CanvasGroup>();
        loadingOverlay.alpha = 0f;
        loadingOverlay.interactable = false;
        loadingOverlay.blocksRaycasts = false;

        GameObject imageObject = new GameObject("BlackScreen");
        imageObject.transform.SetParent(overlayObject.transform, false);

        Image image = imageObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private IEnumerator FadeLoadingOverlay(float targetAlpha, float duration)
    {
        EnsureLoadingOverlay();

        if (loadingOverlay == null)
            yield break;

        float startAlpha = loadingOverlay.alpha;

        loadingOverlay.blocksRaycasts = targetAlpha > 0f;

        if (duration <= 0f)
        {
            SetLoadingOverlayAlpha(targetAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetLoadingOverlayAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }

        SetLoadingOverlayAlpha(targetAlpha);
    }

    private void SetLoadingOverlayAlpha(float alpha)
    {
        if (loadingOverlay == null)
            return;

        loadingOverlay.alpha = Mathf.Clamp01(alpha);
        loadingOverlay.blocksRaycasts = loadingOverlay.alpha > 0.001f;
        loadingOverlay.interactable = false;
    }
}

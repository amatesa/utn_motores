using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string testScene = "TestScene";
    [SerializeField] private string firstLevelScene = "Level_1";
    [SerializeField] private string secondLevelScene = "Level_2";
    [SerializeField] private string thirdLevelScene = "Level_3";
    [SerializeField] private string victoryOverScene = "VictoryScene";
    [SerializeField] private string gameOverScene = "GameOverScene";

    [Header("Player Data")]
    public int PlayerLives;
    public int MaxPlayerLives = 4;

    private bool hasSceneTransition = false;

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
        SceneManager.LoadScene(testScene);
    }

    public void LoadLevel(string sceneName, string spawnID)
    {
        hasSceneTransition = true;

        LevelSpawnManager.SetNextSpawn(spawnID);
        Debug.Log("SET SPAWN: " + spawnID);

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasSceneTransition)
            return;

        StartCoroutine(FinalReposition());

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
        SceneManager.LoadScene(gameOverScene);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TriggerVictory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(victoryOverScene);
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
}

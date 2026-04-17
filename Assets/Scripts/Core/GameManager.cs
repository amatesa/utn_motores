using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
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
        //SceneManager.LoadScene(firstLevelScene);
    }

    public void LoadLevel(string sceneName, string spawnID)
    {
        LevelSpawnManager.SetNextSpawn(spawnID);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        RepositionPlayer();
    }

    private void RepositionPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[GameManager] Player not found in scene");
            return;
        }

        LevelSpawnPoint[] points = FindObjectsByType<LevelSpawnPoint>(FindObjectsSortMode.None);

        foreach (var point in points)
        {
            if (point.SpawnID == LevelSpawnManager.NextSpawnID)
            {
                player.transform.position = point.transform.position;
                player.transform.rotation = point.transform.rotation;
                return;
            }
        }

        Debug.LogWarning("[GameManager] No matching spawn point found for ID: " + LevelSpawnManager.NextSpawnID);
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
}

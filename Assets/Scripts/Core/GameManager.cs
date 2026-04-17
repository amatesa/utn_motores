using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameOverScene = "GameOverScene";

    private GameObject player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                DontDestroyOnLoad(player);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // LEVEL SYSTEM
    // =========================

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
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        LevelSpawnPoint[] points = FindObjectsByType<LevelSpawnPoint>(FindObjectsSortMode.None);

        foreach (var point in points)
        {
            if (point.SpawnID == LevelSpawnManager.NextSpawnID)
            {
                player.transform.position = point.transform.position;
                player.transform.rotation = point.transform.rotation;
                break;
            }
        }
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

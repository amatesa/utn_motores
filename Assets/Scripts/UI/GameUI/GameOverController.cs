using UnityEngine;

public class GameOverController : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1f;
    }

    public void OnRetryPressed()
    {
        GameManager.Instance.StartGame();
    }

    public void OnMainMenuPressed()
    {
        GameManager.Instance.LoadMainMenu();
    }

    public void OnQuitPressed()
    {
        GameManager.Instance.QuitGame();
    }
}

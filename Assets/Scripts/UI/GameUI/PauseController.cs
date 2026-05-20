using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject overlay;

    [Header("Options")]
    [SerializeField] private OptionsController optionsController;

    [Header("Audio")]
    [SerializeField] private AudioSource pauseMusic; 

    private bool isPaused = false;

    private void OnEnable()
    {
        if (pauseAction != null)
            pauseAction.action.Enable();
    }

    private void OnDisable()
    {
        if (pauseAction != null)
            pauseAction.action.Disable();
    }

    private void Update()
    {
        if (pauseAction != null && pauseAction.action.triggered)
        {
            TogglePause();
        }
    }

    // =========================
    // CORE
    // =========================

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        overlay.SetActive(true);

        // AUDIO GLOBAL PAUSE
        AudioListener.pause = true;

        // EXCEPCIÓN: música de pausa
        if (pauseMusic != null)
        {
            pauseMusic.ignoreListenerPause = true;
            pauseMusic.loop = true;
            pauseMusic.Play();
        }

        // CURSOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        overlay.SetActive(false);

        if (optionsController != null)
            optionsController.CloseOptions();

        // AUDIO RESUME
        AudioListener.pause = false;

        if (pauseMusic != null)
        {
            pauseMusic.Stop();
        }

        // CURSOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // =========================
    // UI BUTTONS
    // =========================

    public void OnContinuePressed()
    {
        ResumeGame();
    }

    public void OnOptionsPressed()
    {
        optionsController.OpenOptions();
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameManager.Instance.LoadMainMenu();
    }
}

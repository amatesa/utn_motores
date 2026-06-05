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

    public bool IsPaused => isPaused;

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
        if (isPaused)
            return;

        isPaused = true;
  

        // Aplicar snapshot ANTES de congelar
        if (AudioSnapshotController.Instance != null)
            AudioSnapshotController.Instance.SetPause();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (overlay != null)
            overlay.SetActive(true);

        if (pauseMusic != null)
        {
            pauseMusic.loop = true;

            if (!pauseMusic.isPlaying)
                pauseMusic.Play();
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (overlay != null)
            overlay.SetActive(false);

        if (optionsController != null)
            optionsController.CloseOptions();

        if (pauseMusic != null)
            pauseMusic.Stop();

        // Forzar salida de pausa
        if (AudioSnapshotController.Instance != null)
            AudioSnapshotController.Instance.SetExploration();

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
        if (optionsController != null)
            optionsController.OpenOptions();
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;

        if (pauseMusic != null)
            pauseMusic.Stop();

        GameManager.Instance.LoadMainMenu();
    }
}

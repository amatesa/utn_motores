using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer introVideo;

    [Header("UI")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject coverImage;
    [SerializeField] private GameObject skipMessage;
    [SerializeField] private AudioSource menuMusic;

    [Header("Input")]
    [SerializeField] private InputActionReference skipAction;

    private bool isPlaying = false;
    private bool videoReady = false;

    private void OnEnable()
    {
        if (skipAction != null)
            skipAction.action.Enable();
    }

    private void OnDisable()
    {
        if (skipAction != null)
            skipAction.action.Disable();
    }

    private void Start()
    {
        if (introVideo != null)
        {
            introVideo.gameObject.SetActive(true); 
            introVideo.Prepare(); 

            introVideo.prepareCompleted += OnVideoPrepared;
            introVideo.loopPointReached += OnVideoFinished;
        }

        if (coverImage != null)
            coverImage.SetActive(true);

        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);

        if (skipMessage != null)
            skipMessage.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying || skipAction == null)
            return;

        if (skipAction.action.triggered)
        {
            SkipVideo();
        }
    }

    // =========================
    // PLAY BUTTON
    // =========================
    public void OnPlayPressed()
    {
        if (isPlaying || !videoReady)
            return;

        isPlaying = true;

        if (menuMusic != null)
            menuMusic.Stop(); // ← CLAVE

        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (coverImage != null)
            coverImage.SetActive(false);

        if (skipMessage != null)
            skipMessage.SetActive(true);

        introVideo.Play();
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        videoReady = true;
        Debug.Log("Video READY");
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        GameManager.Instance.StartGame();
    }

    private void SkipVideo()
    {
        introVideo.Stop();
        GameManager.Instance.StartGame();
    }


    // =========================
    // OPTIONS
    // =========================
 
    [SerializeField] private OptionsController optionsController;

    public void OnOptionsPressed()
    {
        optionsController.OpenOptions();
    }

    // =========================
    // QUIT
    // =========================
    public void OnQuitPressed()
    {
        GameManager.Instance.QuitGame();
    }
}

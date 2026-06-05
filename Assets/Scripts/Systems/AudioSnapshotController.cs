using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioState
{
    Exploration,
    Chase,
    Hidden,
    Narration,
    Document,
    Pause
}

public class AudioSnapshotController : MonoBehaviour
{
    public static AudioSnapshotController Instance { get; private set; }

    [Header("Snapshots")]
    [SerializeField] private AudioMixerSnapshot explorationSnapshot;
    [SerializeField] private AudioMixerSnapshot chaseSnapshot;
    [SerializeField] private AudioMixerSnapshot hiddenSnapshot;
    [SerializeField] private AudioMixerSnapshot narrationSnapshot;
    [SerializeField] private AudioMixerSnapshot documentSnapshot;
    [SerializeField] private AudioMixerSnapshot pauseSnapshot;

    [Header("Observed State")]
    [SerializeField] private ShadowEnemyBrain enemy;
    [SerializeField] private IntroBookSequence introBookSequence;
    [SerializeField] private InspectableDocumentViewer documentViewer;
    [SerializeField] private PauseController pauseController;

    [Header("Transition")]
    [SerializeField] private float transitionTime = 0.5f;
    [SerializeField] private float pauseTransitionTime = 0f;
    [SerializeField] private float referenceRefreshInterval = 2f;

    private AudioState currentState;
    private bool hasCurrentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Solo funciona si el GameObject es root.
        if (transform.parent == null)
            DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RefreshMissingReferences();
        ApplyEvaluatedState();
        StartCoroutine(RefreshMissingReferencesRoutine());
    }

    private void Update()
    {
        ApplyEvaluatedState();
    }

    public void SetExploration()
    {
        TransitionTo(AudioState.Exploration);
    }

    public void SetChase()
    {
        TransitionTo(AudioState.Chase);
    }

    public void SetHidden()
    {
        TransitionTo(AudioState.Hidden);
    }

    public void SetNarration()
    {
        TransitionTo(AudioState.Narration);
    }

    public void SetDocument()
    {
        TransitionTo(AudioState.Document);
    }

    public void SetPause()
    {
        TransitionTo(AudioState.Pause);
    }

    private IEnumerator RefreshMissingReferencesRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(
            Mathf.Max(0.25f, referenceRefreshInterval)
        );

        while (true)
        {
            yield return wait;
            RefreshMissingReferences();
        }
    }

    private void RefreshMissingReferences()
    {
        if (enemy == null)
            enemy = FindFirstObjectByType<ShadowEnemyBrain>();

        if (introBookSequence == null)
            introBookSequence = FindFirstObjectByType<IntroBookSequence>();

        if (documentViewer == null)
            documentViewer = FindFirstObjectByType<InspectableDocumentViewer>();

        if (pauseController == null)
            pauseController = FindFirstObjectByType<PauseController>();
    }

    private void ApplyEvaluatedState()
    {
        TransitionTo(EvaluateState());
    }

    private AudioState EvaluateState()
    {
        if (pauseController != null && pauseController.IsPaused)
            return AudioState.Pause;

        if (introBookSequence != null && introBookSequence.IsOpen)
            return AudioState.Narration;

        if (documentViewer != null && documentViewer.IsOpen)
            return AudioState.Document;

        if (PlayerHideState.Instance != null &&
            PlayerHideState.Instance.IsHidden)
            return AudioState.Hidden;

        if (enemy != null &&
            enemy.CurrentState == EnemyState.Chase)
            return AudioState.Chase;

        return AudioState.Exploration;
    }

    private void TransitionTo(AudioState state)
    {
        if (hasCurrentState && currentState == state)
            return;

        AudioMixerSnapshot snapshot = GetSnapshot(state);

        Debug.Log(
            $"TRANSITION -> {state} | Snapshot = {(snapshot != null ? snapshot.name : "NULL")}"
        );

        if (snapshot == null)
            return;

        currentState = state;
        hasCurrentState = true;

        float duration =
            state == AudioState.Pause
                ? pauseTransitionTime
                : transitionTime;

        snapshot.TransitionTo(duration);
    }

    private AudioMixerSnapshot GetSnapshot(AudioState state)
    {
        switch (state)
        {
            case AudioState.Chase:
                return chaseSnapshot;

            case AudioState.Hidden:
                return hiddenSnapshot;

            case AudioState.Narration:
                return narrationSnapshot;

            case AudioState.Document:
                return documentSnapshot;

            case AudioState.Pause:
                return pauseSnapshot;

            default:
                return explorationSnapshot;
        }
    }
}

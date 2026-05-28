using System;
using UnityEngine;

/// <summary>
/// Runtime pacing state service for Silent Escape.
/// Future horror systems can subscribe to phase changes instead of polling
/// unrelated gameplay systems or coupling directly to scene progression.
/// </summary>
[DisallowMultipleComponent]
public class GamePhaseSystem : MonoBehaviour
{
    /// <summary>
    /// Global runtime instance for lightweight access from gameplay systems.
    /// </summary>
    public static GamePhaseSystem Instance { get; private set; }

    /// <summary>
    /// Raised whenever the game phase changes.
    /// Parameters are old phase, then new phase.
    /// </summary>
    public event Action<GamePhase, GamePhase> OnPhaseChanged;

    [Header("Runtime State")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Intro;

    [Header("Debug")]
    [SerializeField] private bool logPhaseChanges = true;

    /// <summary>
    /// Current high-level gameplay phase.
    /// </summary>
    public GamePhase CurrentPhase => currentPhase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Changes the current game phase and notifies listeners.
    /// Repeated calls with the same phase are ignored.
    /// </summary>
    public void SetPhase(GamePhase newPhase)
    {
        if (currentPhase == newPhase)
            return;

        GamePhase oldPhase = currentPhase;
        currentPhase = newPhase;

        if (logPhaseChanges)
            Debug.Log($"[GamePhaseSystem] Phase changed: {oldPhase} -> {newPhase}");

        OnPhaseChanged?.Invoke(oldPhase, newPhase);
    }

    /// <summary>
    /// Returns true while the game is in the opening phase.
    /// </summary>
    public bool IsIntroPhase()
    {
        return currentPhase == GamePhase.Intro;
    }

    /// <summary>
    /// Returns true once the game should apply active horror pressure.
    /// </summary>
    public bool IsDangerPhase()
    {
        return currentPhase == GamePhase.KeyHunt ||
               currentPhase == GamePhase.Escape ||
               currentPhase == GamePhase.FinalEscape;
    }

    /// <summary>
    /// Returns true during escape-oriented phases.
    /// </summary>
    public bool IsEscapePhase()
    {
        return currentPhase == GamePhase.Escape ||
               currentPhase == GamePhase.FinalEscape;
    }

    [ContextMenu("Set Phase/Intro")]
    private void SetIntroPhase()
    {
        SetPhase(GamePhase.Intro);
    }

    [ContextMenu("Set Phase/Exploration")]
    private void SetExplorationPhase()
    {
        SetPhase(GamePhase.Exploration);
    }

    [ContextMenu("Set Phase/Key Hunt")]
    private void SetKeyHuntPhase()
    {
        SetPhase(GamePhase.KeyHunt);
    }

    [ContextMenu("Set Phase/Escape")]
    private void SetEscapePhase()
    {
        SetPhase(GamePhase.Escape);
    }

    [ContextMenu("Set Phase/Final Escape")]
    private void SetFinalEscapePhase()
    {
        SetPhase(GamePhase.FinalEscape);
    }
}

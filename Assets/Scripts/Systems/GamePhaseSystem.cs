using System;
using UnityEngine;


[DisallowMultipleComponent]
public class GamePhaseSystem : MonoBehaviour
{

    public static GamePhaseSystem Instance { get; private set; }


    public event Action<GamePhase, GamePhase> OnPhaseChanged;

    [Header("Runtime State")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Intro;

    [Header("Debug")]
    [SerializeField] private bool logPhaseChanges = true;


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

    public bool IsIntroPhase()
    {
        return currentPhase == GamePhase.Intro;
    }


    public bool IsDangerPhase()
    {
        return currentPhase == GamePhase.KeyHunt ||
               currentPhase == GamePhase.Escape ||
               currentPhase == GamePhase.FinalEscape;
    }


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

using UnityEngine;

public class KeyPhaseTrigger : MonoBehaviour
{
    [Header("Phase Trigger")]
    [SerializeField] private GamePhase phaseToActivate = GamePhase.KeyHunt;

[SerializeField] private bool logChanges = true;

    public void TriggerPhase()
    {
        if (GamePhaseSystem.Instance == null)
        {
            Debug.LogWarning("[KeyPhaseTrigger] GamePhaseSystem not found.");
            return;
        }

        GamePhase currentPhase = GamePhaseSystem.Instance.CurrentPhase;

        if (currentPhase == phaseToActivate)
            return;

        GamePhaseSystem.Instance.SetPhase(phaseToActivate);

        if (logChanges)
        {
            Debug.Log($"[KeyPhaseTrigger] Phase changed: {currentPhase} -> {phaseToActivate}");
        }
    }


}

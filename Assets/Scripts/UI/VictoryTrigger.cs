using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[VictoryTrigger] Player reached goal");

        GameManager.Instance.TriggerVictory();
    }
}

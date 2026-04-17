using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private string spawnID;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[TRIGGER] Entered by: " + other.name);

        if (!other.CompareTag("Player"))
            return;

        Debug.Log("[TRIGGER] PLAYER DETECTED");

        GameManager.Instance.LoadLevel(targetScene, spawnID);
    }
}

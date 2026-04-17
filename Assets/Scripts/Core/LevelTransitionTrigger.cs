using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string targetScene;
    [SerializeField] private string spawnID;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameManager.Instance.LoadLevel(targetScene, spawnID);
    }
}

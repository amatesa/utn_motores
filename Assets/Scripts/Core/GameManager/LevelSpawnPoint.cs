using UnityEngine;

public class LevelSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnID;

    public string SpawnID => spawnID;
}

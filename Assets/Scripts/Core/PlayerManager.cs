using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public Transform ActivePlayer { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PlayerManager] Duplicate detected → destroying " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[PlayerManager] Instance set → " + gameObject.name);
    }

    public void SetActivePlayer(Transform player)
    {
        ActivePlayer = player;
        Debug.Log("[PlayerManager] ActivePlayer SET → " + player.name);
    }
}

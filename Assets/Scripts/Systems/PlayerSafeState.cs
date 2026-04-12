using UnityEngine;

public class PlayerSafeState : MonoBehaviour
{
    public static PlayerSafeState Instance;

    public bool isSafe = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetSafe(bool value)
    {
        isSafe = value;
    }
}

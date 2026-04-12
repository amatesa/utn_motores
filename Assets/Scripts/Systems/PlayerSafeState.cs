using UnityEngine;

public class PlayerSafeState : MonoBehaviour
{
    public static PlayerSafeState Instance;

    [SerializeField] private bool isSafe = false;
    public bool IsSafe => isSafe;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSafe(bool value)
    {
        if (isSafe == value) return;

        isSafe = value;

        Debug.Log("[SAFE STATE] Player isSafe = " + isSafe);
    }
}

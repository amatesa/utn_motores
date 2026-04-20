using UnityEngine;

public class PlayerHideState : MonoBehaviour
{
    public static PlayerHideState Instance;

    public bool IsHidden { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Hide()
    {
        IsHidden = true;
        Debug.Log("[PLAYER] HIDING");
    }

    public void Unhide()
    {
        IsHidden = false;
        Debug.Log("[PLAYER] UNHIDE");
    }
}

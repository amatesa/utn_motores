using UnityEngine;

public class LanternPickup : MonoBehaviour
{
    [SerializeField] private GameObject playerLantern;

    [SerializeField] private LanternController lanternController;

    [SerializeField] private float initialFuelSeconds = 60f;

    [SerializeField]
    private string pickupThought =
        "This might keep it away.";

    private bool collected;

    public void CollectLantern()
    {
        if (collected)
            return;

        collected = true;

        if (playerLantern != null)
            playerLantern.SetActive(true);


        if (lanternController != null)
            lanternController.ActivateLanternWithFuel(initialFuelSeconds);


        if (ThoughtPopupSystem.Instance != null)
            ThoughtPopupSystem.Instance.ShowThought(pickupThought);

        Destroy(gameObject);
    }
}

using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[SAFE AREA] ENTER");

            PlayerSafeState.Instance.SetSafe(true);
            NoiseSystem.Instance.ClearSounds();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[SAFE AREA] EXIT");

            PlayerSafeState.Instance.SetSafe(false);
        }
    }
}

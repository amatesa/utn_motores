using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ENTER SAFE AREA");
            PlayerSafeState.Instance.SetSafe(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSafeState.Instance.SetSafe(false);
            Debug.Log("Exit SAFE");
        }
    }
}

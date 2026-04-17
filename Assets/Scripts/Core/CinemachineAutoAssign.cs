using UnityEngine;
using Unity.Cinemachine;

public class CinemachineAutoAssign : MonoBehaviour
{
    private CinemachineCamera cam;

    void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
    }

    void Start()
    {
        AssignTarget();
    }

    void AssignTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[Cinemachine] Player not found");
            return;
        }

        // IMPORTANTE: usar el target correcto (no el root)
        Transform target = player.transform;

        cam.Follow = target;
        cam.LookAt = target;
    }
}

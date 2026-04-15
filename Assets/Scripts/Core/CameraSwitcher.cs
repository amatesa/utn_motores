using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera firstPersonCam;
    public CinemachineCamera thirdPersonCam;

    private bool isFirstPerson = false;

    void Start()
    {
        ActivateThirdPerson();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isFirstPerson)
                ActivateThirdPerson();
            else
                ActivateFirstPerson();
        }
    }

    void ActivateFirstPerson()
    {
        firstPersonCam.Priority = 20;
        thirdPersonCam.Priority = 10;
        isFirstPerson = true;
    }

    void ActivateThirdPerson()
    {
        firstPersonCam.Priority = 10;
        thirdPersonCam.Priority = 20;
        isFirstPerson = false;
    }
}

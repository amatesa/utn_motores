using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera firstPersonCam;
    public CinemachineCamera thirdPersonCam;
    public FirstPersonBodyVisibilityController bodyVisibilityController;

    private bool isFirstPerson = false;

    private void Awake()
    {
        if (bodyVisibilityController == null)
        {
            bodyVisibilityController = GetComponentInParent<FirstPersonBodyVisibilityController>();
        }
    }

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
        bodyVisibilityController?.SetFirstPersonMode(true);
        isFirstPerson = true;
    }

    void ActivateThirdPerson()
    {
        firstPersonCam.Priority = 10;
        thirdPersonCam.Priority = 20;
        bodyVisibilityController?.SetFirstPersonMode(false);
        isFirstPerson = false;
    }
}

using Unity.Cinemachine;
using UnityEngine;
using StarterAssets;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera firstPersonCam;
    public CinemachineCamera thirdPersonCam;
    public FirstPersonBodyVisibilityController bodyVisibilityController;
    public StarterAssetsInputs inputSource;
    public ThirdPersonController thirdPersonController;

    private bool isFirstPerson = false;

    private void Awake()
    {
        if (inputSource == null)
        {
            inputSource = FindFirstObjectByType<StarterAssetsInputs>();
        }

        if (bodyVisibilityController == null)
        {
            bodyVisibilityController = GetComponentInParent<FirstPersonBodyVisibilityController>();
        }

        if (bodyVisibilityController == null)
        {
            bodyVisibilityController = FindFirstObjectByType<FirstPersonBodyVisibilityController>();
        }

        if (thirdPersonController == null)
        {
            thirdPersonController = GetComponentInParent<ThirdPersonController>();
        }

        if (thirdPersonController == null)
        {
            thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
        }
    }

    void Start()
    {
        ActivateThirdPerson();
    }

    void Update()
    {
        if (inputSource != null && inputSource.switchCamera)
        {
            inputSource.switchCamera = false;

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
        thirdPersonController?.SetFirstPersonMode(true);
        isFirstPerson = true;
    }

    void ActivateThirdPerson()
    {
        firstPersonCam.Priority = 10;
        thirdPersonCam.Priority = 20;
        bodyVisibilityController?.SetFirstPersonMode(false);
        thirdPersonController?.SetFirstPersonMode(false);
        isFirstPerson = false;
    }
}

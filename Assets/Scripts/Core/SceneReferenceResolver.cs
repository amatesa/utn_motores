using UnityEngine;
using Unity.Cinemachine;
using StarterAssets;

public class SceneReferenceResolver : MonoBehaviour
{
    void Start()
    {
        ResolveAll();
    }

    void ResolveAll()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[Resolver] Player not found");
            return;
        }

        ResolveCamera(player);
        ResolveEnemy(player);
        ResolveUI(player);
    }

    void ResolveCamera(GameObject player)
    {
        var cams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            Transform target = player.transform;

            var controller = player.GetComponent<ThirdPersonController>();

            if (controller != null && controller.CinemachineCameraTarget != null)
            {
                target = controller.CinemachineCameraTarget.transform;
            }

            cam.Follow = target;
            cam.LookAt = target;
        }

        var switcher = FindAnyObjectByType<CameraSwitcher>();

        if (switcher != null)
        {
            switcher.inputSource = player.GetComponent<StarterAssetsInputs>();
            switcher.thirdPersonController = player.GetComponent<ThirdPersonController>();
        }
    }

    void ResolveEnemy(GameObject player)
    {
        var enemies = FindObjectsByType<ShadowEnemyBrain>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            //enemy.SetTarget(player.transform);
        }

        var perceptions = FindObjectsByType<EnemyPerception>(FindObjectsSortMode.None);

        foreach (var p in perceptions)
        {
            p.SetTarget(player.transform);
        }

        var audio = FindObjectsByType<EnemyPresenceAudio>(FindObjectsSortMode.None);

        foreach (var a in audio)
        {
            //a.SetPlayer(player.transform);
        }
    }

    void ResolveUI(GameObject player)
    {
        var staminaUI = FindAnyObjectByType<StaminaUI>();
        if (staminaUI != null)
        {
            staminaUI.SetSystem(player.GetComponent<PlayerStaminaSystem>());
        }

        var lifeUI = FindAnyObjectByType<LifeUI>();
        if (lifeUI != null)
        {
            //lifeUI.SetSystem(player.GetComponent<PlayerLifeSystem>());
        }

        var ecg = FindAnyObjectByType<NoiseECG_UI>();
        if (ecg != null)
        {
            //ecg.SetPlayer(player.transform);
        }
    }
}

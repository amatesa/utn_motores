using UnityEngine;

[DisallowMultipleComponent]
public class EnemyLanternResponder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShadowEnemyBrain brain;
    [SerializeField] private EnemyAggressionController aggressionController;

    [Header("Lantern")]
    [SerializeField] private LanternProtectionZone lanternZone;

    [Header("Behavior")]
    [SerializeField] private float retreatTriggerDistance = 4f;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled;

    private Transform player;

    private void Awake()
    {
        if (brain == null)
            brain = GetComponent<ShadowEnemyBrain>();

        if (aggressionController == null)
            aggressionController = GetComponent<EnemyAggressionController>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (brain == null)
            return;

        if (lanternZone == null)
            return;

        if (!lanternZone.IsActive)
            return;

        if (!lanternZone.ContainsEnemy(gameObject))
            return;

        float protection =
            aggressionController != null
                ? aggressionController.GetEffectiveLanternProtection(
                    lanternZone.ProtectionEfficiency
                )
                : lanternZone.ProtectionEfficiency;

        if (protection <= 0.05f)
            return;

        brain.RequestLanternRetreat();

        if (debugEnabled)
        {
            Debug.Log("[ENEMY] Lantern retreat triggered.");
        }
    }
    public bool IsRepelling()
    {
        if (lanternZone == null)
            return false;

        if (!lanternZone.IsActive)
            return false;

        return lanternZone.ContainsEnemy(gameObject);
    }


}


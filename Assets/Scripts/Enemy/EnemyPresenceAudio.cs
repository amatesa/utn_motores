using UnityEngine;

public class EnemyPresenceAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private ShadowEnemy enemy; //usamos estado real

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource whisperSource;
    [SerializeField] private AudioSource attackSource;

    [Header("Distances")]
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private float whisperDistance = 8f;
    [SerializeField] private float attackDistance = 2.5f;

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 3f;

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        bool isThreat = enemy != null && enemy.IsThreatActive();

        HandleAmbient();
        HandleWhisper(distance, isThreat);
        HandleAttack(distance, isThreat);
    }

    // =========================
    // AMBIENT → SIEMPRE ACTIVO
    // =========================
    void HandleAmbient()
    {
        ambientSource.volume = Mathf.Lerp(ambientSource.volume, 0.6f, Time.deltaTime * fadeSpeed);
    }

    // =========================
    // WHISPER → CERCA Y NO CHASE
    // =========================
    void HandleWhisper(float distance, bool isThreat)
    {
        float target = 0f;

        if (!isThreat && distance < whisperDistance)
        {
            target = Mathf.InverseLerp(whisperDistance, attackDistance, distance);
        }

        whisperSource.volume = Mathf.Lerp(whisperSource.volume, target, Time.deltaTime * fadeSpeed);
    }

    // =========================
    // ATTACK → CHASE O MUY CERCA
    // =========================
    void HandleAttack(float distance, bool isThreat)
    {
        float target = 0f;

        if (isThreat)
        {
            target = 2f;
        }

        attackSource.volume = Mathf.Lerp(attackSource.volume, target, Time.deltaTime * fadeSpeed);
    }
}

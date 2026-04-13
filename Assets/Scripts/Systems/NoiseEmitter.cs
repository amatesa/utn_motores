using UnityEngine;

public class NoiseEmitter : MonoBehaviour
{
    [Header("Noise Settings")]
    [SerializeField] private float intensity = 10f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private bool useRandomOffset = true;

    [Header("Trigger Settings")]
    [SerializeField] private bool emitOnStart = false;
    [SerializeField] private bool emitOnCollision = false;
    [SerializeField] private bool emitOnTriggerEnter = false;

    [Header("Debug")]
    [SerializeField] private bool debugEnabled = true;

    private float lastEmitTime;

    void Start()
    {
        if (emitOnStart)
        {
            Emit();
        }
    }

    public void Emit()
    {
        if (Time.time - lastEmitTime < cooldown)
            return;

        lastEmitTime = Time.time;

        Vector3 pos = transform.position;

        if (useRandomOffset)
        {
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = 0;
            pos += offset;
        }

        NoiseSystem.Instance.EmitSound(pos, intensity);

        if (debugEnabled)
        {
            Debug.Log($"[NoiseEmitter] EMIT → {gameObject.name} intensity={intensity}");
        }
    }

    // =========================
    // COLLISION
    // =========================
    void OnCollisionEnter(Collision collision)
    {
        if (!emitOnCollision) return;

        Emit();
    }

    // =========================
    // TRIGGER
    // =========================
    void OnTriggerEnter(Collider other)
    {
        if (!emitOnTriggerEnter) return;

        if (other.CompareTag("Player"))
        {
            Emit();
        }
    }
}

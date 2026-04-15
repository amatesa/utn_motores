using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 4f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    [Header("AI")]
    [SerializeField] private bool enemyCanOpen = false;
    [SerializeField] private float autoCloseDelay = 10f;

    [Header("Interaction")]
    [SerializeField] private InteractionEmitter interactionEmitter;
    [SerializeField] private Transform player;

    private bool isOpen = false;
    private bool isMoving = false;
    private float targetAngle;

    private Coroutine autoCloseCoroutine;

    private void Update()
    {
        if (isMoving)
        {
            RotateDoor();
        }
    }

    // =========================
    // PLAYER (compatibilidad)
    // =========================
    public void UseDoor()
    {
        UseDoor(null);
    }

    // =========================
    // NUEVO SISTEMA (con instigator)
    // =========================
    public void UseDoor(GameObject instigator)
    {
        if (isMoving) return;

        isOpen = !isOpen;

        float direction = GetOpenDirection(instigator);

        targetAngle = isOpen ? openAngle * direction : 0f;

        isMoving = true;

        // 🔊 SONIDO
        if (isOpen)
            audioSource.PlayOneShot(openSound);
        else
            audioSource.PlayOneShot(closeSound);

        // 🔊 EMITIR RUIDO
        if (interactionEmitter != null)
        {
            if (instigator != null)
                interactionEmitter.Interact(instigator);
            else
                interactionEmitter.Interact();
        }

        // 🤖 AUTO CLOSE SI ENEMY
        if (instigator != null && instigator.CompareTag("Enemy") && isOpen)
        {
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());
        }
    }

    private IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (!isMoving && isOpen)
        {
            UseDoor(null); // cierre normal
        }
    }

    private float GetOpenDirection(GameObject instigator)
    {
        Transform reference = player;

        if (instigator != null)
        {
            reference = instigator.transform;
        }

        Vector3 toAgent = reference.position - pivot.position;
        toAgent.y = 0;

        Vector3 right = pivot.right;

        float dot = Vector3.Dot(-right, toAgent);

        Debug.Log("[DOOR] DOT (" + reference.name + "): " + dot);

        return (dot > 0) ? 1f : -1f;
    }

    private void RotateDoor()
    {
        float currentY = pivot.localEulerAngles.y;

        float angle = Mathf.LerpAngle(currentY, targetAngle, Time.deltaTime * speed);
        pivot.localEulerAngles = new Vector3(0, angle, 0);

        if (Mathf.Abs(Mathf.DeltaAngle(currentY, targetAngle)) < 0.5f)
        {
            pivot.localEulerAngles = new Vector3(0, targetAngle, 0);
            isMoving = false;
        }
    }

    // =========================
    // USO PARA IA
    // =========================
    public void OpenDoorFromAI(Transform agent)
    {
        if (!enemyCanOpen) return;

        if (isMoving) return;

        UseDoor(agent.gameObject);
    }
}

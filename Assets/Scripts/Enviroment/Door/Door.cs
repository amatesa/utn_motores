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

    [Header("AI")]
    [SerializeField] private bool enemyCanOpen = false;

    [Header("Interaction")]
    [SerializeField] private InteractionEmitter interactionEmitter;
    [SerializeField] private Transform player;

    private bool isOpen = false;
    private bool isMoving = false;
    private float targetAngle;

    private void Update()
    {
        if (isMoving)
        {
            RotateDoor();
        }
    }

    public void UseDoor()
    {
        if (isMoving) return;

        isOpen = !isOpen;

        float direction = GetOpenDirection();

        targetAngle = isOpen ? openAngle * direction : 0f;

        isMoving = true;

        audioSource.PlayOneShot(openSound);

        interactionEmitter?.Interact();
    }

    private float GetOpenDirection()
    {
        Vector3 toPlayer = player.position - pivot.position;

        // Ignorar altura (muy importante)
        toPlayer.y = 0;

        Vector3 right = pivot.right;

        float dot = Vector3.Dot(-right, toPlayer);

        Debug.Log("[DOOR] DOT RIGHT: " + dot);

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

    // USO PARA IA
    public void OpenDoorFromAI(Transform agent)
    {
        if (!enemyCanOpen) return;

        if (isMoving) return;

        isOpen = !isOpen;

        Vector3 toAgent = agent.position - pivot.position;
        float dot = Vector3.Dot(pivot.forward, toAgent);

        float direction = (dot > 0) ? -1f : 1f;

        targetAngle = isOpen ? openAngle * direction : 0f;

        isMoving = true;

        audioSource.PlayOneShot(openSound);

        interactionEmitter?.Interact();
    }
}

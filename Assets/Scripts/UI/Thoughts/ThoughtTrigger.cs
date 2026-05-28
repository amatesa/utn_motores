using UnityEngine;

[DisallowMultipleComponent]
public class ThoughtTrigger : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string message;

    [SerializeField] private float duration = 4f;

    [SerializeField] private int priority = 0;

    [SerializeField] private bool canInterrupt = false;

    [SerializeField] private bool showOnlyOnce = true;

    [SerializeField] private ThoughtType thoughtType = ThoughtType.Flavor;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (showOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        if (ThoughtPopupSystem.Instance == null)
            return;

        ThoughtPopupSystem.Instance.ShowThought(
            message,
            duration,
            priority,
            canInterrupt,
            thoughtType
        );
    }
}

using UnityEngine;

public class Lamp : MonoBehaviour
{
    private bool isOn;

    [SerializeField] private GameObject lightObject;
    [SerializeField] private AudioClip onSound;
    [SerializeField] private AudioClip offSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private InteractionEmitter interactionEmitter;
    public void UseLamp()
    {
        isOn = !isOn;

        lightObject.SetActive(isOn);

        if (isOn)
        {
            audioSource.PlayOneShot(onSound);

            if (interactionEmitter != null)
                interactionEmitter.Interact();
        }
        else
        {
            Debug.Log("[LAMP] OFF");
            audioSource.PlayOneShot(offSound);
        }
    }
}

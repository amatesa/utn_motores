using UnityEngine;
using UnityEngine.UI;

public class LifeUI : MonoBehaviour
{
    [SerializeField] private Slider lifeSlider;
    [SerializeField] private PlayerLifeSystem lifeSystem;

    void Update()
    {
        if (lifeSystem == null || lifeSlider == null)
            return;

        lifeSlider.value = lifeSystem.CurrentLifeNormalized;
    }
}

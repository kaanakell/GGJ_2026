using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Health Bar")]
    public Slider healthSlider;
    public PlayerHealth playerHealth;

    [Header("Mask Icon")]
    public Image currentMaskIcon;
    public Sprite tentacleSprite;
    public Sprite clawSprite;

    private void Start()
    {
        if (playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.maxHealth;
        }
    }

    private void Update()
    {
        if (playerHealth != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, playerHealth.CurrentHealth, Time.deltaTime * 10f);
        }
    }

    public void UpdateMaskIcon(MaskManager.MaskType activeMask)
    {
        if (activeMask == MaskManager.MaskType.Tentacle)
            currentMaskIcon.sprite = tentacleSprite;
        else
            currentMaskIcon.sprite = clawSprite;

        currentMaskIcon.rectTransform.localScale = Vector3.one * 1.2f;
    }
}
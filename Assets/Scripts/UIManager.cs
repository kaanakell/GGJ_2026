using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Header("Health Bar")]
    public Slider healthSlider;
    public PlayerHealth playerHealth;

    [Header("Mask Icon")]
    public Image currentMaskIcon;
    public Sprite tentacleSprite;
    public Sprite clawSprite;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;

    void Awake()
    {
        Instance = this;
    }

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

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShowLevelComplete()
    {
        levelCompletePanel.SetActive(true);
    }

    public void Restart()
    {
        GameStateManager.Instance.RestartLevel();
    }

    public void MainMenu()
    {
        GameStateManager.Instance.LoadMainMenu();
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
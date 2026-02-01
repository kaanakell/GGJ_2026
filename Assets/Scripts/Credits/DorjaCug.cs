using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class DorjaCug : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("LinkedIn URL")]
    [SerializeField] private string itchIoUrl = "https://www.linkedin.com/in/dorja-cug-983504251/";

    [Header("Text Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.cyan;

    private TextMeshProUGUI tmpText;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            normalColor = tmpText.color;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Application.OpenURL(itchIoUrl);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tmpText != null)
        {
            tmpText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tmpText != null)
        {
            tmpText.color = normalColor;
        }
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PariaAkhavass : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Website URL")]
    [SerializeField] private string websiteUrl = "https://www.pariaakhavass.com/";

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
        Application.OpenURL(websiteUrl);
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
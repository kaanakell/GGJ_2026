using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class CagdasKaratas : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler  
{
    [Header("Itch.io URL")]
    [SerializeField] private string itchIoUrl = "https://cagkar07.itch.io/";

    [Header("Text Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

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
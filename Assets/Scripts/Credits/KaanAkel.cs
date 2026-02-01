using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class KaanAkel : MonoBehaviour, 
    IPointerClickHandler, 
    IPointerEnterHandler, 
    IPointerExitHandler
{
    [Header("Itch.io URL")]
    [SerializeField] private string itchIoUrl = "https://kaanakel.itch.io/";

    [Header("Text Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.cyan;

    private TextMeshProUGUI tmpText;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        normalColor = tmpText.color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Application.OpenURL(itchIoUrl);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tmpText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tmpText.color = normalColor;
    }
}

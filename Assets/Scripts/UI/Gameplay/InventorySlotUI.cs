using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedHighlight;
    [SerializeField] private TMP_Text inputGuide;
    
    public void SetSelected(bool selected) => selectedHighlight?.SetActive(!selected);

    public void SetIcon(Sprite sprite)
    {
        if (!iconImage) return;
        iconImage.sprite = sprite;
        iconImage.enabled = sprite != null;
    }
}
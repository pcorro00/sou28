using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 스탯 아이콘 UI
/// </summary>
public class StatIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI valueText;

    public void Setup(Sprite icon, string value)
    {
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }

        if (valueText != null)
        {
            valueText.text = value;
        }
    }
}

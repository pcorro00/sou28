using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TraitSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color inactiveColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color bronze = new Color(0.8f, 0.5f, 0.3f);
    [SerializeField] private Color silver = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color gold = new Color(1f, 0.84f, 0f);

    private string traitName;
    private TraitInfo traitInfo;
    public int CurrentCount => traitInfo != null ? traitInfo.currentCount : 0;

    public void Setup(string name, TraitInfo info, Sprite icon)
    {
        traitName = name;
        traitInfo = info;
        UpdateUI(icon);
    }

    private void UpdateUI(Sprite icon)
    {
        if (traitInfo == null) return;

        // 아이콘
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }

        // 이름
        if (nameText != null)
        {
            nameText.text = traitName;
        }

        // 카운트
        if (countText != null)
        {
            int nextThreshold = GetNextThreshold();
            if (nextThreshold > 0)
            {
                countText.text = $"{traitInfo.currentCount}/{nextThreshold}";
            }
            else
            {
                countText.text = $"{traitInfo.currentCount}";
            }
        }

        // 배경 색상
        if (backgroundImage != null)
        {
            backgroundImage.color = GetColorByLevel();
        }
    }

    private int GetNextThreshold()
    {
        if (traitInfo.thresholds.Count == 0) return 0;

        foreach (int threshold in traitInfo.thresholds)
        {
            if (traitInfo.currentCount < threshold)
            {
                return threshold;
            }
        }

        return traitInfo.thresholds[traitInfo.thresholds.Count - 1];
    }

    private Color GetColorByLevel()
    {
        switch (traitInfo.activeLevel)
        {
            case 0: return inactiveColor;
            case 1: return bronze;
            case 2: return silver;
            case 3: return gold;
            default: return inactiveColor;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 레벨업 유닛 선택 슬롯
/// </summary>
public class LevelUpChoiceSlot : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI tierText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    private UnitData unitData;
    private Button button;
    private LevelUpUI levelUpUI;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
    
    /// <summary>
    /// 슬롯 설정
    /// </summary>
    public void Setup(UnitData data, LevelUpUI ui)
    {
        unitData = data;
        levelUpUI = ui;
        UpdateUI();
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (unitData == null) return;
        
        // 아이콘
        if (iconImage != null)
        {
            if (unitData.icon != null)
            {
                iconImage.sprite = unitData.icon;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.color = unitData.displayColor;
            }
        }
        
        // 배경색 (Tier에 따라)
        if (backgroundImage != null)
        {
            backgroundImage.color = GetTierColor(unitData.tier);
        }
        
        // 이름
        if (nameText != null)
        {
            nameText.text = unitData.unitName;
        }
        
        // Tier
        if (tierText != null)
        {
            tierText.text = $"Tier {(int)unitData.tier}";
        }
        
        // 설명
        if (descriptionText != null)
        {
            descriptionText.text = unitData.description;
        }
    }
    
    /// <summary>
    /// Tier 색상
    /// </summary>
    private Color GetTierColor(UnitTier tier)
    {
        switch (tier)
        {
            case UnitTier.Tier1:
                return new Color(0.3f, 0.3f, 0.3f); // 어두운 회색
            case UnitTier.Tier2:
                return new Color(0.2f, 0.4f, 0.6f); // 어두운 파랑
            case UnitTier.Tier3:
                return new Color(0.4f, 0.2f, 0.6f); // 어두운 보라
            case UnitTier.Tier4:
                return new Color(0.6f, 0.4f, 0.2f); // 어두운 주황
            case UnitTier.Tier5:
                return new Color(0.6f, 0.2f, 0.2f); // 어두운 빨강
            default:
                return Color.gray;
        }
    }
    
    /// <summary>
    /// 클릭 이벤트
    /// </summary>
    private void OnClick()
    {
        if (unitData != null && levelUpUI != null)
        {
            levelUpUI.SelectUnit(unitData);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 유닛 슬롯 UI
/// </summary>
public class UnitSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI tierText;

    private UnitData unitData;
    private Button button;
    private bool isSelected = false;
    private UnitInventoryUI inventoryUI;

    public UnitData UnitData => unitData;
    public bool IsSelected => isSelected;

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
    public void Setup(UnitData data, UnitInventoryUI ui)
    {
        unitData = data;
        inventoryUI = ui;
        UpdateUI();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        Debug.Log($">>> UpdateUI called for {(unitData != null ? unitData.unitName : "NULL")}");

        if (unitData == null)
        {
            Debug.LogWarning("UnitData is NULL in UpdateUI!");
            // 빈 슬롯
            if (iconImage != null)
                iconImage.enabled = false;

            if (backgroundImage != null)
                backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            if (button != null)
                button.interactable = false;

            if (nameText != null)
                nameText.text = "";

            if (tierText != null)
                tierText.text = "";
        }
        else
        {
            Debug.Log($">>> Displaying {unitData.unitName}, Icon: {(unitData.icon != null ? "YES" : "NO")}");

            // 유닛 정보 표시
            if (iconImage != null)
            {
                iconImage.enabled = true;

                if (unitData.icon != null)
                {
                    iconImage.sprite = unitData.icon;
                    Debug.Log($">>> Icon sprite set for {unitData.unitName}");
                }
                else
                {
                    // 아이콘 없으면 색상으로 구분
                    iconImage.sprite = null;
                    Debug.Log($">>> No icon for {unitData.unitName}, using color only");
                }

                iconImage.color = unitData.displayColor;
            }
            else
            {
                Debug.LogError("iconImage is NULL!");
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = GetTierColor(unitData.tier);
            }

            if (nameText != null)
            {
                nameText.text = unitData.unitName;
                Debug.Log($">>> Name text set: {unitData.unitName}");
            }

            if (tierText != null)
            {
                tierText.text = $"T{(int)unitData.tier}";
            }

            if (button != null)
                button.interactable = true;
        }

        SetSelected(false);
    }

    /// <summary>
    /// Tier에 따른 배경색
    /// </summary>
    private Color GetTierColor(UnitTier tier)
    {
        switch (tier)
        {
            case UnitTier.Tier1:
                return new Color(0.8f, 0.8f, 0.8f); // 회색
            case UnitTier.Tier2:
                return new Color(0.6f, 0.8f, 1f); // 파란색
            case UnitTier.Tier3:
                return new Color(0.8f, 0.6f, 1f); // 보라색
            case UnitTier.Tier4:
                return new Color(1f, 0.8f, 0.4f); // 주황색
            case UnitTier.Tier5:
                return new Color(1f, 0.6f, 0.6f); // 빨간색
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// 클릭 이벤트
    /// </summary>
    private void OnClick()
    {
        if (unitData == null || inventoryUI == null) return;

        // 인벤토리 UI에 선택 토글
        inventoryUI.ToggleUnitSelection(this);
    }

    /// <summary>
    /// 선택 상태 설정
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectedFrame != null)
        {
            selectedFrame.SetActive(selected);
        }

        if (backgroundImage != null && unitData != null)
        {
            if (selected)
            {
                // 선택 시 더 밝게
                Color tierColor = GetTierColor(unitData.tier);
                backgroundImage.color = tierColor * 1.3f;
            }
            else
            {
                // 기본 색상
                backgroundImage.color = GetTierColor(unitData.tier);
            }
        }
    }
}
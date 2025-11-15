using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 레벨업 시 유닛 선택 UI
/// </summary>
public class LevelUpUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private GameObject contentPanel;  // 숨길 대상
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button hideButton;  // 숨기기/보이기 버튼
    [SerializeField] private TextMeshProUGUI hideButtonText;  // 버튼 텍스트

    [Header("Choice Slot Prefab")]
    [SerializeField] private GameObject choiceSlotPrefab;

    [Header("Available Units")]
    [SerializeField] private List<UnitData> tier1Units;  // Tier 1 유닛들
    [SerializeField] private List<UnitData> tier2Units;  // Tier 2 유닛들

    private List<GameObject> currentChoiceSlots = new List<GameObject>();
    private List<UnitData> currentChoices = new List<UnitData>();

    // 토글 상태
    private bool isLevelUpActive = false;  // 레벨업 선택 중인지
    private bool isPanelVisible = true;    // 패널이 보이는지

    // Singleton
    public static LevelUpUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 초기 상태: 숨김
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // GameManager의 레벨업 이벤트 구독
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnLevelUp += ShowLevelUpChoices;
        }

        Debug.Log("LevelUp UI initialized");
    }

    private void OnDestroy()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnLevelUp -= ShowLevelUpChoices;
        }
    }

    private void Update()
    {
        // 레벨업 선택 중일 때만 ESC 키로 토글
        if (isLevelUpActive && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }
    }

    /// <summary>
    /// 패널 토글 (숨기기/보이기)
    /// </summary>
    public void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;

        // ContentPanel만 숨기기 (버튼은 계속 보임)
        if (contentPanel != null)
        {
            contentPanel.SetActive(isPanelVisible);
        }

        // 버튼 텍스트 변경
        UpdateHideButtonText();

        Debug.Log(isPanelVisible ? "Level up panel shown" : "Level up panel hidden (checking field)");
    }

    /// <summary>
    /// 숨기기 버튼 텍스트 업데이트
    /// </summary>
    private void UpdateHideButtonText()
    {
        if (hideButtonText != null)
        {
            if (isPanelVisible)
            {
                hideButtonText.text = "숨기기 ▼";
            }
            else
            {
                hideButtonText.text = "보이기 ▲";
            }
        }
    }

    /// <summary>
    /// 레벨업 선택창 표시
    /// </summary>
    public void ShowLevelUpChoices(int level)
    {
        Debug.Log($"=== Level Up to {level}! ===");

        // 게임 일시정지
        Time.timeScale = 0;

        // 상태 설정
        isLevelUpActive = true;
        isPanelVisible = true;

        // 패널 표시
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }

        // ContentPanel도 표시
        if (contentPanel != null)
        {
            contentPanel.SetActive(true);
        }

        // 랜덤 유닛 3개 선택
        currentChoices = GetRandomUnits(3, level);

        // UI 생성
        CreateChoiceSlots(currentChoices);

        // 버튼 텍스트 초기화
        UpdateHideButtonText();

        Debug.Log("Press ESC to hide/show panel and check your field");
    }

    /// <summary>
    /// 랜덤 유닛 가져오기
    /// </summary>
    private List<UnitData> GetRandomUnits(int count, int playerLevel)
    {
        List<UnitData> choices = new List<UnitData>();
        List<UnitData> availableUnits = new List<UnitData>();

        // 플레이어 레벨에 따라 등장 가능한 유닛 결정
        if (playerLevel >= 1)
        {
            availableUnits.AddRange(tier1Units);
        }

        if (playerLevel >= 3)
        {
            availableUnits.AddRange(tier2Units);
        }

        // 유닛이 없으면 기본 유닛 사용
        if (availableUnits.Count == 0)
        {
            Debug.LogWarning("No units available! Using Tier 1 units.");
            availableUnits.AddRange(tier1Units);
        }

        // 랜덤 선택 (중복 가능)
        for (int i = 0; i < count; i++)
        {
            if (availableUnits.Count > 0)
            {
                int randomIndex = Random.Range(0, availableUnits.Count);
                choices.Add(availableUnits[randomIndex]);
            }
        }

        return choices;
    }

    /// <summary>
    /// 선택 슬롯 생성
    /// </summary>
    private void CreateChoiceSlots(List<UnitData> units)
    {
        // 기존 슬롯 제거
        ClearChoiceSlots();

        // 새 슬롯 생성
        for (int i = 0; i < units.Count; i++)
        {
            UnitData unit = units[i];
            GameObject slotObj = Instantiate(choiceSlotPrefab, choicesContainer);

            // 슬롯 설정
            LevelUpChoiceSlot slot = slotObj.GetComponent<LevelUpChoiceSlot>();
            if (slot != null)
            {
                slot.Setup(unit, this);
            }

            currentChoiceSlots.Add(slotObj);
        }

        Debug.Log($"Created {units.Count} choice slots");
    }

    /// <summary>
    /// 슬롯 제거
    /// </summary>
    private void ClearChoiceSlots()
    {
        foreach (GameObject slot in currentChoiceSlots)
        {
            if (slot != null)
            {
                Destroy(slot);
            }
        }
        currentChoiceSlots.Clear();
    }

    /// <summary>
    /// 유닛 선택
    /// </summary>
    public void SelectUnit(UnitData selectedUnit)
    {
        if (selectedUnit == null)
        {
            Debug.LogError("Selected unit is null!");
            return;
        }

        Debug.Log($"Selected: {selectedUnit.unitName}");

        // 인벤토리에 추가
        UnitInventory inventory = UnitInventory.Instance;
        if (inventory != null)
        {
            inventory.AddUnit(selectedUnit);
        }

        // 패널 닫기
        CloseLevelUpPanel();
    }

    /// <summary>
    /// 레벨업 패널 닫기
    /// </summary>
    private void CloseLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // 상태 리셋
        isLevelUpActive = false;
        isPanelVisible = true;

        // 게임 재개
        Time.timeScale = 1;

        // 슬롯 정리
        ClearChoiceSlots();

        Debug.Log("Level up panel closed");
    }
}
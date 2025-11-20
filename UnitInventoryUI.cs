using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 모바일용 유닛 인벤토리 UI
/// </summary>
public class UnitInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Button openButton;  // 하단 유닛 버튼
    [SerializeField] private Button closeButton;

    [Header("Action Buttons")]
    [SerializeField] private Button combineButton;
    [SerializeField] private Button deployButton;
    [SerializeField] private Button cancelButton;

    [Header("Tier Containers")]
    [SerializeField] private Transform tier1Container;
    [SerializeField] private Transform tier2Container;
    [SerializeField] private Transform tier3Container;
    [SerializeField] private Transform tier4Container;
    [SerializeField] private Transform tier5Container;

    [Header("Prefabs")]
    [SerializeField] private GameObject unitSlotPrefab;

    [Header("Systems")]
    private UnitInventory inventory;

    // 슬롯 관리
    private List<UnitSlotUI> allSlots = new List<UnitSlotUI>();

    // 선택된 유닛들 (조합용)
    private List<UnitSlotUI> selectedSlots = new List<UnitSlotUI>();

    // Singleton
    public static UnitInventoryUI Instance { get; private set; }

    private void Awake()
    {
        Debug.Log($">>> UnitInventoryUI Awake called on: {gameObject.name}");

        if (Instance == null)
        {
            Instance = this;
            Debug.Log($">>> Instance set to: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($">>> Duplicate UnitInventoryUI found on: {gameObject.name}, destroying...");
            Destroy(gameObject);
            return;
        }
    }
   
    private void Start()
    {
        // 시스템 찾기
        inventory = UnitInventory.Instance;

        if (inventory == null)
        {
            Debug.LogError("UnitInventory not found!");
            return;
        }

        // 이벤트 연결
        inventory.OnInventoryChanged += RefreshInventory;


        if (openButton != null)
        {
            openButton.onClick.AddListener(OpenInventory);
            Debug.Log("OpenButton listener added!");
        }
        else
        {
            Debug.LogError("openButton is NULL! Not connected in Inspector!");
        }

        if (openButton != null)
            openButton.onClick.AddListener(OpenInventory);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseInventory);

        if (combineButton != null)
            combineButton.onClick.AddListener(TryCombine);

        if (deployButton != null)
            deployButton.onClick.AddListener(DeploySelected);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(ClearSelection);

        // 초기 상태
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        UpdateActionButtons();

        Debug.Log("Unit Inventory UI initialized");
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshInventory;
        }
    }

    /// <summary>
    /// 인벤토리 열기
    /// </summary>
    public void OpenInventory()
    {
        Debug.Log("=== OpenInventory CALLED! ===");
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            RefreshInventory();
            Debug.Log("Unit inventory opened");
        }
    }

    /// <summary>
    /// 인벤토리 닫기
    /// </summary>
    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            ClearSelection();
            Debug.Log("Unit inventory closed");
        }
    }

    /// <summary>
    /// 인벤토리 새로고침
    /// </summary>
    private void RefreshInventory()
    {
        Debug.Log("=== Refreshing Unit Inventory ===");

        // 기존 슬롯 제거
        ClearSlots();

        // Tier별로 유닛 슬롯 생성
        for (int tier = 1; tier <= 5; tier++)
        {
            List<UnitData> units = inventory.GetUnitsByTier((UnitTier)tier);
            Transform container = GetContainerForTier(tier);

            Debug.Log($">>> Tier {tier}: {units.Count} units, Container: {(container != null ? "OK" : "NULL")}");

            if (container == null)
            {
                Debug.LogError($"Container for Tier {tier} is NULL!");
                continue;
            }

            foreach (UnitData unit in units)
            {
                Debug.Log($">>> Creating slot for: {unit.unitName}");
                CreateSlot(unit, container);
            }
        }
    }

    /// <summary>
    /// Tier에 맞는 컨테이너 가져오기
    /// </summary>
    private Transform GetContainerForTier(int tier)
    {
        switch (tier)
        {
            case 1: return tier1Container;
            case 2: return tier2Container;
            case 3: return tier3Container;
            case 4: return tier4Container;
            case 5: return tier5Container;
            default: return null;
        }
    }

    /// <summary>
    /// 슬롯 생성
    /// </summary>
    private void CreateSlot(UnitData unit, Transform container)
    {
        if (unitSlotPrefab == null || container == null)
        {
            Debug.LogError($"Slot prefab or container is null! Prefab: {unitSlotPrefab != null}, Container: {container != null}");
            return;
        }

        Debug.Log($">>> Instantiating slot for {unit.unitName}...");
        GameObject slotObj = Instantiate(unitSlotPrefab, container);
        Debug.Log($">>> Slot created: {slotObj.name}");

        // 크기 강제 설정
        RectTransform rect = slotObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(150, 150);
        }

        UnitSlotUI slot = slotObj.GetComponent<UnitSlotUI>();
        if (slot != null)
        {
            Debug.Log($">>> Setting up slot UI for {unit.unitName}");
            slot.Setup(unit, this);
            allSlots.Add(slot);
            Debug.Log($">>> Slot setup complete! Total slots: {allSlots.Count}");
        }
        else
        {
            Debug.LogError("UnitSlotUI component not found on slot prefab!");
        }
    }

    /// <summary>
    /// 슬롯 제거
    /// </summary>
    private void ClearSlots()
    {
        foreach (UnitSlotUI slot in allSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        allSlots.Clear();
        selectedSlots.Clear();
    }

    /// <summary>
    /// 유닛 선택 토글
    /// </summary>
    public void ToggleUnitSelection(UnitSlotUI slot)
    {
        if (slot == null) return;

        // 이미 선택되어 있으면 선택 해제
        if (selectedSlots.Contains(slot))
        {
            selectedSlots.Remove(slot);
            slot.SetSelected(false);
            Debug.Log($"Deselected: {slot.UnitData.unitName} ({selectedSlots.Count}/5)");
        }
        else
        {
            // 최대 5개까지만 선택
            if (selectedSlots.Count >= 5)
            {
                Debug.LogWarning("Maximum 5 units can be selected!");
                return;
            }

            selectedSlots.Add(slot);
            slot.SetSelected(true);
            Debug.Log($"Selected: {slot.UnitData.unitName} ({selectedSlots.Count}/5)");
        }

        UpdateActionButtons();
    }

    /// <summary>
    /// 선택 취소
    /// </summary>
    public void ClearSelection()
    {
        foreach (UnitSlotUI slot in selectedSlots)
        {
            if (slot != null)
            {
                slot.SetSelected(false);
            }
        }
        selectedSlots.Clear();
        Debug.Log("Selection cleared");
        UpdateActionButtons();
    }

    /// <summary>
    /// 액션 버튼 상태 업데이트
    /// </summary>
    private void UpdateActionButtons()
    {
        int selectedCount = selectedSlots.Count;

        // 조합 버튼: 2~5개 선택 시 활성화
        if (combineButton != null)
        {
            combineButton.interactable = (selectedCount >= 2 && selectedCount <= 5);
        }

        // 배치 버튼: 정확히 1개 선택 시 활성화
        if (deployButton != null)
        {
            deployButton.interactable = (selectedCount == 1);
        }

        // 취소 버튼: 1개 이상 선택 시 활성화
        if (cancelButton != null)
        {
            cancelButton.interactable = (selectedCount > 0);
        }
    }

    /// <summary>
    /// 조합 시도
    /// </summary>
    public void TryCombine()
    {
        if (selectedSlots.Count < 2)
        {
            Debug.LogWarning("Select at least 2 units to combine!");
            return;
        }

        if (selectedSlots.Count > 5)
        {
            Debug.LogWarning("Maximum 5 units can be combined!");
            return;
        }

        // 선택된 유닛 데이터 추출
        List<UnitData> selectedUnits = new List<UnitData>();
        foreach (UnitSlotUI slot in selectedSlots)
        {
            if (slot != null && slot.UnitData != null)
            {
                selectedUnits.Add(slot.UnitData);
            }
        }

        // 업그레이드 시도
        UnitUpgrader upgrader = UnitUpgrader.Instance;
        if (upgrader == null)
        {
            Debug.LogError("UnitUpgrader not found!");
            return;
        }

        UpgradeResult result = upgrader.UpgradeUnits(selectedUnits);

        if (result.isSuccess)
        {
            Debug.Log($"✅ Upgrade successful: {result.resultUnit.unitName}");

            // 사용한 유닛 제거
            inventory.RemoveUnits(selectedUnits);

            // 결과 유닛 추가
            inventory.AddUnit(result.resultUnit);

            // 선택 초기화
            ClearSelection();

            // UI 새로고침
            RefreshInventory();

            // TODO: 성공 팝업 표시
        }
        else
        {
            Debug.LogWarning($"❌ Upgrade failed: {result.message}");
            // TODO: 실패 메시지 표시
        }
    }

    /// <summary>
    /// 선택된 유닛 배치
    /// </summary>
    public void DeploySelected()
    {
        if (selectedSlots.Count != 1)
        {
            Debug.LogWarning("Select exactly 1 unit to deploy!");
            return;
        }

        UnitSlotUI slot = selectedSlots[0];
        if (slot == null || slot.UnitData == null) return;

        Debug.Log($"Deploying: {slot.UnitData.unitName}");

        // 배치 시스템에 전달
        UnitPlacer placer = FindFirstObjectByType<UnitPlacer>();
        if (placer != null)
        {
            // 인벤토리에서 제거
            inventory.RemoveUnit(slot.UnitData);

            // 배치 모드 시작 (메서드명 변경!)
            placer.StartPlacementWithUnitData(slot.UnitData);

            // 인벤토리 닫기
            CloseInventory();

            Debug.Log($"Started deployment mode for {slot.UnitData.unitName}");
        }
        else
        {
            Debug.LogError("UnitPlacer not found!");
        }
    }
}
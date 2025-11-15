using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 유닛 시스템 테스트용 매니저
/// </summary>
public class UnitTestManager : MonoBehaviour
{
    [Header("테스트용 유닛 데이터")]
    [SerializeField] private UnitData testWarrior;      // 전사
    [SerializeField] private UnitData testArcher;       // 궁수
    [SerializeField] private UnitData testMage;         // 마법사

    [Header("선택된 유닛들 (조합용)")]
    private List<UnitData> selectedUnits = new List<UnitData>();

    private UnitInventory inventory;
    private UnitUpgrader upgrader;

    private void Start()
    {
        inventory = UnitInventory.Instance;
        upgrader = UnitUpgrader.Instance;

        Debug.Log("=== Unit Test Manager Ready ===");
        Debug.Log("Press '1': Add Warrior");
        Debug.Log("Press '2': Add Archer");
        Debug.Log("Press '3': Add Mage");
        Debug.Log("Press 'Q': Select Warrior for upgrade");
        Debug.Log("Press 'W': Select Archer for upgrade");
        Debug.Log("Press 'E': Select Mage for upgrade");
        Debug.Log("Press 'U': Upgrade selected units");
        Debug.Log("Press 'P': Print inventory");
        Debug.Log("Press 'C': Clear selection");
        Debug.Log("Press 'X': Clear inventory");
    }

    private void Update()
    {
        // 유닛 추가
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddTestUnit(testWarrior);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddTestUnit(testArcher);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddTestUnit(testMage);
        }

        // 조합용 유닛 선택
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SelectUnitForUpgrade(testWarrior);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SelectUnitForUpgrade(testArcher);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SelectUnitForUpgrade(testMage);
        }

        // 업그레이드 실행
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpgradeSelectedUnits();
        }

        // 인벤토리 출력
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (inventory != null)
            {
                inventory.PrintInventory();
            }
        }

        // 선택 취소
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearSelection();
        }

        // 인벤토리 초기화
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (inventory != null)
            {
                inventory.ClearInventory();
            }
        }
    }

    // 테스트용 유닛 추가
    private void AddTestUnit(UnitData unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("Test unit is null!");
            return;
        }

        if (inventory != null)
        {
            inventory.AddUnit(unit);
        }
    }

    // 조합용 유닛 선택
    private void SelectUnitForUpgrade(UnitData unit)
    {
        if (unit == null) return;

        // 인벤토리에서 해당 타입의 유닛 개수 확인
        var unitsInInventory = inventory.GetUnitsByType(unit.unitType);

        if (unitsInInventory.Count == 0)
        {
            Debug.LogWarning($"No {unit.unitName} in inventory!");
            return;
        }

        // 이미 선택된 같은 타입 유닛 개수 세기
        int alreadySelectedCount = 0;
        foreach (var selectedUnit in selectedUnits)
        {
            if (selectedUnit.unitType == unit.unitType)
            {
                alreadySelectedCount++;
            }
        }

        // 인벤토리에 있는 개수보다 많이 선택할 수 없음
        if (alreadySelectedCount >= unitsInInventory.Count)
        {
            Debug.LogWarning($"All {unit.unitName} units are already selected!");
            return;
        }

        // 최대 5개까지만 선택 가능
        if (selectedUnits.Count >= 5)
        {
            Debug.LogWarning("Maximum 5 units can be selected!");
            return;
        }

        // 선택 (인벤토리에서 해당 인덱스의 유닛)
        UnitData unitToSelect = unitsInInventory[alreadySelectedCount];
        selectedUnits.Add(unitToSelect);

        Debug.Log($"Selected {unitToSelect.unitName} for upgrade ({selectedUnits.Count}/5)");

        // 선택된 유닛들 표시
        Debug.Log($"Current selection: {string.Join(", ", selectedUnits.ConvertAll(u => u.unitName))}");
    }

    // 선택된 유닛들로 업그레이드
    private void UpgradeSelectedUnits()
    {
        if (selectedUnits.Count < 2)
        {
            Debug.LogWarning("Need at least 2 units to upgrade!");
            return;
        }

        if (selectedUnits.Count > 5)
        {
            Debug.LogWarning("Too many units selected! Max 5.");
            return;
        }

        Debug.Log($"Attempting upgrade with {selectedUnits.Count} units...");

        // 업그레이드 시도
        UpgradeResult result = upgrader.UpgradeUnits(selectedUnits);

        if (result.isSuccess)
        {
            Debug.Log($" {result.message}");

            // 사용한 유닛들 인벤토리에서 제거
            inventory.RemoveUnits(selectedUnits);

            // 결과 유닛 인벤토리에 추가
            inventory.AddUnit(result.resultUnit);

            // 선택 초기화
            ClearSelection();
        }
        else
        {
            Debug.LogWarning($" Upgrade failed: {result.message}");
        }
    }

    // 선택 초기화
    private void ClearSelection()
    {
        selectedUnits.Clear();
        Debug.Log("Selection cleared");
    }
}
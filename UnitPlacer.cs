using Unity.AppUI.UI;
using UnityEngine;

/// <summary>
/// PC와 모바일을 모두 지원하는 유닛 배치 관리자
/// </summary>
public class UnitPlacer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridSystem gridSystem;

    [Header("Test Settings")]
    [SerializeField] private GameObject testUnitPrefab;

    [Header("Unit Data")]
    private UnitData currentUnitData;

    private GameObject selectedUnit;
    private bool isPlacingMode = false;

    private void Start()
    {
        if (gridSystem == null)
        {
            gridSystem = FindFirstObjectByType<GridSystem>();
        }

        // 그리드 이벤트 구독
        if (gridSystem != null)
        {
            gridSystem.OnCellClicked += OnCellClicked;
            gridSystem.OnCellRemoveRequested += OnCellRemoveRequested;
            Debug.Log("Grid System connected!");
        }
        else
        {
            Debug.LogError("GridSystem not found!");
        }
    }

    private void OnDestroy()
    {
        if (gridSystem != null)
        {
            gridSystem.OnCellClicked -= OnCellClicked;
            gridSystem.OnCellRemoveRequested -= OnCellRemoveRequested;
        }
    }

    // 셀 클릭 이벤트 (배치)
    private void OnCellClicked(Vector2Int gridPos)
    {
        if (isPlacingMode && selectedUnit != null)
        {
            PlaceUnit(gridPos);
        }
    }

    // 셀 제거 요청 이벤트 (PC: 우클릭, 모바일: 길게 누르기)
    private void OnCellRemoveRequested(Vector2Int gridPos)
    {
        RemoveUnitAt(gridPos);
    }

    // 유닛 배치 시작 (파츠 조합 완료 후 호출)
    public void StartPlacingUnit(GameObject unitPrefab)
    {
        selectedUnit = unitPrefab;
        isPlacingMode = true;
        Debug.Log("Placing mode started. Click/Tap on a grid cell to place unit.");
    }

    // 유닛 배치 실행 (수정됨!)
    private void PlaceUnit(Vector2Int gridPos)
    {
        if (selectedUnit == null)
        {
            Debug.LogWarning("No unit selected to place!");
            return;
        }

        // GridSystem을 통해 배치 시도
        bool success = gridSystem.PlaceUnit(gridPos, selectedUnit);

        if (success)
        {
            // 배치 성공 시 해당 위치의 유닛 가져오기
            GameObject placedUnit = gridSystem.GetUnitAtPosition(gridPos);

            if (placedUnit != null)
            {
                // UnitData가 있으면 스탯 초기화
                InitializeUnitStats(placedUnit);
            }

            Debug.Log($"Unit placed at {gridPos}");

            // 배치 모드 종료
            isPlacingMode = false;
            selectedUnit = null;
        }
        else
        {
            Debug.LogWarning($"Failed to place unit at {gridPos}");
        }
    }

    // 유닛 제거
    private void RemoveUnitAt(Vector2Int gridPos)
    {
        GameObject unit = gridSystem.GetUnitAtPosition(gridPos);

        if (unit != null)
        {
            if (gridSystem.RemoveUnit(gridPos))
            {
                Destroy(unit);
                Debug.Log($"Unit removed from {gridPos}");
            }
        }
    }

    // 배치 모드 취소
    public void CancelPlacingMode()
    {
        isPlacingMode = false;
        selectedUnit = null;
        Debug.Log("Placing mode cancelled.");
    }

    // ========== UnitData 지원 ==========

    /// <summary>
    /// UnitData로 배치 시작
    /// </summary>
    public void StartPlacementWithUnitData(UnitData unitData)
    {
        if (unitData == null)
        {
            Debug.LogError("UnitData is null!");
            return;
        }

        currentUnitData = unitData;

        // 프리팹으로 배치 시작
        if (unitData.unitPrefab != null)
        {
            StartPlacingUnit(unitData.unitPrefab);
        }
        else
        {
            Debug.LogError($"Unit {unitData.unitName} has no prefab!");
        }
    }

    /// <summary>
    /// 배치된 유닛의 스탯 초기화
    /// </summary>
    private void InitializeUnitStats(GameObject placedUnit)
    {
        if (placedUnit == null || currentUnitData == null) return;

        UnitStats stats = placedUnit.GetComponent<UnitStats>();
        if (stats != null)
        {
            stats.InitializeFromUnit(currentUnitData);

            // TraitManager에 등록 (위치값 필요 없음!)
            if (TraitManager.Instance != null)
            {
                TraitManager.Instance.RegisterUnit(stats);
            }

            Debug.Log($"Unit registered for traits: {currentUnitData.unitName}");
        }
    }

    // ========== 테스트용 입력 ==========
    private void Update()
    {
        // Space: 배치 모드 시작
        if (Input.GetKeyDown(KeyCode.Space) && testUnitPrefab != null)
        {
            StartPlacingUnit(testUnitPrefab);
        }

        // ESC: 배치 모드 취소
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacingMode();
        }
    }
}
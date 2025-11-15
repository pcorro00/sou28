using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// PC와 모바일을 모두 지원하는 통합 그리드 배치 시스템
/// </summary>
public class GridSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 5;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    [Header("Visual Settings")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Color placableColor = new Color(0, 1, 0, 0.3f);
    [SerializeField] private Color occupiedColor = new Color(1, 0, 0, 0.3f);
    [SerializeField] private Color hoverColor = new Color(1, 1, 0, 0.5f);

    [Header("Mobile Settings")]
    [SerializeField] private float longPressDuration = 0.5f; // 길게 눌러서 유닛 제거

    private GridCell[,] grid;
    private Dictionary<Vector2Int, GameObject> unitPositions = new Dictionary<Vector2Int, GameObject>();
    private GameObject currentHoverCell;

    // 터치 관련
    private float touchStartTime;
    private Vector2Int touchStartGridPos;
    private bool isTouching;

    private void Start()
    {
        InitializeGrid();
        CreateVisualGrid();
    }

    private void InitializeGrid()
    {
        grid = new GridCell[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = new GridCell(x, y, true);
            }
        }
    }

    private void CreateVisualGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = GridToWorldPosition(x, y);
                GameObject cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
                cell.name = $"Cell_{x}_{y}";

                GridCellVisual cellVisual = cell.GetComponent<GridCellVisual>();
                if (cellVisual != null)
                {
                    cellVisual.Initialize(x, y, this);
                    cellVisual.SetColor(placableColor);
                }

                grid[x, y].visualObject = cell;
            }
        }
    }

    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
            HandleTouchInput();
#else
        HandleMouseInput();
#endif
    }

    // ========== PC 입력 (마우스) ==========
    private void HandleMouseInput()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 마우스 호버 효과
        UpdateHoverEffect(mousePos);

        // 왼쪽 클릭 - 유닛 배치
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(mousePos);
        }

        // 우클릭 - 유닛 제거
        if (Input.GetMouseButtonDown(1))
        {
            HandleRemove(mousePos);
        }
    }

    private void UpdateHoverEffect(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);

        if (IsValidGridPosition(gridPos))
        {
            GameObject cell = grid[gridPos.x, gridPos.y].visualObject;

            if (currentHoverCell != cell)
            {
                // 이전 호버 셀 복구
                if (currentHoverCell != null)
                {
                    GridCellVisual prevVisual = currentHoverCell.GetComponent<GridCellVisual>();
                    Vector2Int prevPos = prevVisual.GridPosition;
                    prevVisual.SetColor(IsCellOccupied(prevPos) ? occupiedColor : placableColor);
                }

                // 새 호버 셀
                currentHoverCell = cell;
                GridCellVisual cellVisual = cell.GetComponent<GridCellVisual>();
                if (!IsCellOccupied(gridPos))
                {
                    cellVisual.SetColor(hoverColor);
                }
            }
        }
    }

    // ========== 모바일 입력 (터치) ==========
    private void HandleTouchInput()
    {
        // 터치가 없으면 리턴
        if (Input.touchCount == 0)
        {
            if (isTouching)
            {
                OnTouchEnded();
            }
            return;
        }

        Touch touch = Input.GetTouch(0);
        Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(touch.position);
        Vector2Int gridPos = WorldToGridPosition(touchWorldPos);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                OnTouchBegan(gridPos);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                OnTouchHold(gridPos);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                OnTouchEnded();
                break;
        }
    }

    private void OnTouchBegan(Vector2Int gridPos)
    {
        isTouching = true;
        touchStartTime = Time.time;
        touchStartGridPos = gridPos;

        Debug.Log($"[TOUCH] Touch began at: {gridPos}"); // 터치 확인용 로그

        if (IsValidGridPosition(gridPos))
        {
            GameObject cell = grid[gridPos.x, gridPos.y].visualObject;
            currentHoverCell = cell;

            GridCellVisual cellVisual = cell.GetComponent<GridCellVisual>();
            if (!IsCellOccupied(gridPos))
            {
                cellVisual.SetColor(hoverColor);
            }

            Debug.Log($"[Touch] Began at: {gridPos}");
        }
    }

    private void OnTouchHold(Vector2Int gridPos)
    {
        // 길게 누르기 감지 (유닛 제거용)
        if (IsValidGridPosition(gridPos) && gridPos == touchStartGridPos)
        {
            float holdTime = Time.time - touchStartTime;

            if (holdTime >= longPressDuration && IsCellOccupied(gridPos))
            {
                HandleRemove(GridToWorldPosition(gridPos.x, gridPos.y));
                isTouching = false; // 이벤트 한 번만 발생
            }
        }
    }

    private void OnTouchEnded()
    {
        if (!isTouching) return;

        isTouching = false;
        float touchDuration = Time.time - touchStartTime;

        // 이전 선택 셀 복구
        if (currentHoverCell != null)
        {
            GridCellVisual cellVisual = currentHoverCell.GetComponent<GridCellVisual>();
            Vector2Int pos = cellVisual.GridPosition;
            cellVisual.SetColor(IsCellOccupied(pos) ? occupiedColor : placableColor);
        }

        // 짧은 탭 = 유닛 배치
        if (touchDuration < longPressDuration && IsValidGridPosition(touchStartGridPos))
        {
            Vector3 worldPos = GridToWorldPosition(touchStartGridPos.x, touchStartGridPos.y);
            HandleClick(worldPos);
        }

        currentHoverCell = null;
    }

    // ========== 공통 처리 ==========
    private void HandleClick(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);

        if (IsValidGridPosition(gridPos))
        {
            if (CanPlaceUnit(gridPos))
            {
                OnCellClicked?.Invoke(gridPos);
                Debug.Log($"Cell clicked: {gridPos}");
            }
            else
            {
                Debug.Log($"Cell {gridPos} is already occupied!");
            }
        }
    }

    private void HandleRemove(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);

        if (IsValidGridPosition(gridPos) && IsCellOccupied(gridPos))
        {
            OnCellRemoveRequested?.Invoke(gridPos);
            Debug.Log($"Remove requested at: {gridPos}");
        }
    }

    // ========== 유닛 관리 ==========
    public bool PlaceUnit(Vector2Int gridPos, GameObject unitPrefab)
    {
        if (!CanPlaceUnit(gridPos))
            return false;

        // Prefab을 실제로 생성! (중요!)
        GameObject unit = Instantiate(unitPrefab);
        unit.name = $"Unit_{gridPos.x}_{gridPos.y}";

        grid[gridPos.x, gridPos.y].isOccupied = true;
        unitPositions[gridPos] = unit;

        GridCellVisual cellVisual = grid[gridPos.x, gridPos.y].visualObject.GetComponent<GridCellVisual>();
        if (cellVisual != null)
        {
            cellVisual.SetColor(occupiedColor);
        }

        unit.transform.position = GridToWorldPosition(gridPos.x, gridPos.y);

        Debug.Log($"Unit instantiated at {unit.transform.position}, GridPos: {gridPos}");

        return true;
    }

    public bool RemoveUnit(Vector2Int gridPos)
    {
        if (!IsValidGridPosition(gridPos) || !grid[gridPos.x, gridPos.y].isOccupied)
            return false;

        grid[gridPos.x, gridPos.y].isOccupied = false;
        unitPositions.Remove(gridPos);

        GridCellVisual cellVisual = grid[gridPos.x, gridPos.y].visualObject.GetComponent<GridCellVisual>();
        if (cellVisual != null)
        {
            cellVisual.SetColor(placableColor);
        }

        return true;
    }

    // ========== 유틸리티 함수 ==========
    public bool CanPlaceUnit(Vector2Int gridPos)
    {
        return IsValidGridPosition(gridPos) &&
               grid[gridPos.x, gridPos.y].isPlaceable &&
               !grid[gridPos.x, gridPos.y].isOccupied;
    }

    public bool IsCellOccupied(Vector2Int gridPos)
    {
        if (!IsValidGridPosition(gridPos))
            return true;
        return grid[gridPos.x, gridPos.y].isOccupied;
    }

    public bool IsValidGridPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth &&
               gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        // 셀의 중심점을 기준으로 위치 계산
        return gridOrigin + new Vector3(x * cellSize + cellSize * 0.5f, y * cellSize + cellSize * 0.5f, 0);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - gridOrigin;
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    public GameObject GetUnitAtPosition(Vector2Int gridPos)
    {
        if (unitPositions.ContainsKey(gridPos))
            return unitPositions[gridPos];
        return null;
    }

    // ========== 이벤트 ==========
    public System.Action<Vector2Int> OnCellClicked;
    public System.Action<Vector2Int> OnCellRemoveRequested;

    // ========== 에디터용 기즈모 ==========
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = gridOrigin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = gridOrigin + new Vector3(x * cellSize, gridHeight * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = gridOrigin + new Vector3(0, y * cellSize, 0);
            Vector3 end = gridOrigin + new Vector3(gridWidth * cellSize, y * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}

[System.Serializable]
public class GridCell
{
    public int x;
    public int y;
    public bool isPlaceable;
    public bool isOccupied;
    public GameObject visualObject;

    public GridCell(int x, int y, bool placeable)
    {
        this.x = x;
        this.y = y;
        this.isPlaceable = placeable;
        this.isOccupied = false;
    }
}
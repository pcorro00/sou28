using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UnitDragHandler : MonoBehaviour
{
    [Header("드래그 설정")]
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private LayerMask gridLayerMask = 1 << 0;

    private UnitStats unitStats;
    private GridSystem gridSystem;
    private bool isDragging = false;
    private bool isHolding = false;
    private float holdTimer = 0f;
    private Vector3 originalPosition;
    private Vector2Int originalGridPos;
    private Camera mainCamera;

    private void Start()
    {
        unitStats = GetComponent<UnitStats>();
        gridSystem = FindFirstObjectByType<GridSystem>();
        mainCamera = Camera.main;
        originalPosition = transform.position;
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0) return;
        if (IsPointerOverUI()) return;

        isHolding = true;
        holdTimer = 0f;
        StartCoroutine(CheckHold());
    }

    private void OnMouseUp()
    {
        if (isDragging)
        {
            HandleDrop();
        }
        else if (isHolding && holdTimer < holdDuration)
        {
            UnitInfoUI infoUI = UnitInfoUI.Instance;
            if (infoUI != null)
            {
                infoUI.ShowUnitInfo(unitStats);
            }
        }

        isHolding = false;
        isDragging = false;
        holdTimer = 0f;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;
    }

    private IEnumerator CheckHold()
    {
        while (isHolding && holdTimer < holdDuration)
        {
            holdTimer += Time.deltaTime;
            yield return null;
        }

        if (isHolding && holdTimer >= holdDuration)
        {
            StartDragging();
        }
    }

    private void StartDragging()
    {
        isDragging = true;
        originalPosition = transform.position;
        originalGridPos = unitStats.GridPosition;

        transform.localScale = Vector3.one * 1.2f;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.7f;
            sr.color = c;
        }

        Debug.Log($"Started dragging {unitStats.CharacterName}");
    }

    private void HandleDrop()
    {
        transform.localScale = Vector3.one;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        if (IsOverInventoryButton())
        {
            ReturnToInventory();
        }
        else if (TryPlaceOnGrid())
        {
            Debug.Log("Unit moved to new position");
        }
        else
        {
            transform.position = originalPosition;
            Debug.Log("Invalid drop position - returning to original");
        }
    }

    private bool IsOverInventoryButton()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.name == "InventoryButton" ||
                result.gameObject.CompareTag("InventoryButton"))
            {
                return true;
            }
        }
        return false;
    }

    private bool TryPlaceOnGrid()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector2Int gridPos = gridSystem.GetGridPosition(mousePos);

        if (gridSystem.IsValidPosition(gridPos) && !gridSystem.IsOccupied(gridPos))
        {
            gridSystem.RemoveUnit(originalGridPos);

            // PlaceUnit 매개변수 순서 수정
            gridSystem.PlaceUnit(gridPos, gameObject);  // 순서 바꿈
            transform.position = gridSystem.GetWorldPosition(gridPos);
            unitStats.Initialize(gridPos);

            return true;
        }

        return false;
    }

    private void ReturnToInventory()
    {
        Debug.Log($"Returning {unitStats.CharacterName} to inventory");

        UnitInventory inventory = UnitInventory.Instance;
        if (inventory != null)
        {
            UnitData unitData = Resources.Load<UnitData>($"Units/{unitStats.UnitType}");
            if (unitData != null)
            {
                inventory.AddUnit(unitData);
                gridSystem.RemoveUnit(originalGridPos);
                Destroy(gameObject);
            }
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
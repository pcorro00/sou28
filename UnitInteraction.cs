using UnityEngine;

public class UnitInteraction : MonoBehaviour
{
    [Header("설정")]
    public float holdThreshold = 0.25f; // 롱 프레스 시간
    public LayerMask unitLayer; // 유닛 레이어

    private bool isPressed = false;
    private bool isDragging = false;
    private float pressTime = 0f;
    private Vector3 offset;

    // ▼▼▼ [오류 해결] 없어서 에러 났던 변수들을 여기에 선언합니다 ▼▼▼
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;
    private Vector2Int currentGridPos; // 현재 내가 있는 그리드 좌표
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // ▼▼▼ 변수 초기화 (이게 없으면 나중에 Null 에러 남) ▼▼▼
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 게임 시작 시, 내가 어느 타일 위에 있는지 계산해서 기억해둠
        // (주의: GridSystem이 먼저 켜져 있어야 함)
        Invoke(nameof(InitGridPosition), 0.1f);
    }

    void InitGridPosition()
    {
        if (GridSystem.Instance != null)
        {
            currentGridPos = GridSystem.Instance.WorldToGridPosition(transform.position);
        }
    }

    void Update()
    {
        // 1. 마우스 누름
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, unitLayer);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isPressed = true;
                isDragging = false;
                pressTime = Time.time;
                offset = transform.position - (Vector3)mousePos;
            }
        }

        // 2. 홀드 체크
        if (isPressed && !isDragging)
        {
            if (Time.time - pressTime > holdThreshold)
            {
                StartDragging();
            }
        }

        // 3. 드래그 이동
        if (isDragging)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
        }

        // 4. 마우스 뗌
        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            isPressed = false;

            if (isDragging)
            {
                EndDragging();
            }
            else
            {
                // 클릭으로 간주 (스탯창)
                if (UnitInfoUI.Instance != null)
                {
                    UnitStats myStats = GetComponent<UnitStats>();
                    if (myStats) UnitInfoUI.Instance.ShowUnitInfo(myStats);
                }
            }
        }
    }

    void StartDragging()
    {
        isDragging = true;

        // 시각 효과: 맨 위로 그리기
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 100;
        }
        transform.localScale = Vector3.one * 1.2f;
    }

    // ▼▼▼ 아까 수정한 그 함수 (이제 변수가 있어서 에러 안 남) ▼▼▼
    void EndDragging()
    {
        isDragging = false;

        // 시각 효과 원상복구
        transform.localScale = Vector3.one;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        // GridSystem에게 이동 요청
        if (GridSystem.Instance != null)
        {
            // TryMoveUnit 함수 호출 (나 자신, 이전 좌표, 현재 마우스 놓은 위치)
            bool moved = GridSystem.Instance.TryMoveUnit(gameObject, currentGridPos, transform.position);

            if (moved)
            {
                // 이동 성공! -> 나의 현재 좌표(currentGridPos)를 갱신
                currentGridPos = GridSystem.Instance.WorldToGridPosition(transform.position);
            }
            // 실패하면 TryMoveUnit 함수가 알아서 원래 위치로 되돌림
        }
        else
        {
            Debug.LogError("GridSystem이 없습니다!");
        }
    }
}
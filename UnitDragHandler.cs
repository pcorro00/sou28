using UnityEngine;

public class UnitDragHandler : MonoBehaviour
{
    [Header("설정")]
    public float holdThreshold = 0.25f; // 0.25초 이상 누르면 드래그
    public float dragScale = 1.2f;      // 드래그 중 크기 확대

    private bool isPressed = false;
    private bool isDragging = false;
    private float pressTime = 0f;

    private Vector3 offset;
    private Vector3 originalScale;
    private int originalSortOrder;

    private SpriteRenderer spriteRenderer;
    private Camera mainCam;

    // 현재 유닛이 있는 그리드 좌표를 기억해야 함
    private Vector2Int currentGridPos;

    void Start()
    {
        mainCam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // 시작할 때 내 그리드 좌표를 알아냄
        // (주의: 유닛 생성 시점에 GridSystem에 등록된 상태여야 정확함)
        Invoke(nameof(SyncGridPosition), 0.1f);
    }

    void SyncGridPosition()
    {
        if (GridSystem.Instance != null)
        {
            currentGridPos = GridSystem.Instance.WorldToGridPosition(transform.position);
        }
    }

    void Update()
    {
        // 1. 마우스/터치 시작
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // 내가 클릭되었는지 확인
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isPressed = true;
                isDragging = false;
                pressTime = Time.time;

                // 마우스와 유닛 중심 간의 거리 차이 계산 (자연스러운 드래그)
                offset = transform.position - (Vector3)mousePos;

                // 현재 그리드 위치 갱신 (혹시 모르니)
                SyncGridPosition();
            }
        }

        // 2. 누르고 있는 중 (홀드 체크)
        if (isPressed && !isDragging)
        {
            if (Time.time - pressTime > holdThreshold)
            {
                StartDragging();
            }
        }

        // 3. 드래그 중 이동 로직
        if (isDragging)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, 0);
        }

        // 4. 마우스/터치 뗌
        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            isPressed = false;

            if (isDragging)
            {
                EndDragging();
            }
            else
            {
                // 드래그 안 하고 뗐음 -> 클릭(스탯창)
                HandleClick();
            }
        }
    }

    void StartDragging()
    {
        isDragging = true;

        // 시각적 효과: 크기 키우기 & 맨 위로 그리기
        transform.localScale = originalScale * dragScale;
        if (spriteRenderer)
        {
            originalSortOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 100; // UI보다 앞에 보이게
        }
    }

    void EndDragging()
    {
        isDragging = false;

        // 시각적 효과 복구
        transform.localScale = originalScale;
        if (spriteRenderer)
        {
            spriteRenderer.sortingOrder = originalSortOrder;
        }

        // GridSystem에게 배치 요청
        if (GridSystem.Instance != null)
        {
            bool success = GridSystem.Instance.TryMoveUnit(gameObject, currentGridPos, transform.position);

            if (success)
            {
                // 이동 성공했으면 내 내부 좌표 업데이트
                currentGridPos = GridSystem.Instance.WorldToGridPosition(transform.position);
            }
            // 실패했으면 TryMoveUnit 안에서 알아서 원래 위치로 돌려보냄
        }
        else
        {
            // 그리드 시스템이 없으면 그냥 원래 자리로
            transform.position = GridSystem.Instance.GridToWorldPosition(currentGridPos.x, currentGridPos.y);
        }
    }

    void HandleClick()
    {
        Debug.Log("유닛 클릭됨: 스탯창 열기");

        // 님 코드에 있는 UnitInfoUI 사용
        UnitStats myStats = GetComponent<UnitStats>();
        if (myStats != null && UnitInfoUI.Instance != null)
        {
            UnitInfoUI.Instance.ShowUnitInfo(myStats);
        }
    }
}
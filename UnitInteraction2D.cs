using UnityEngine;

public class UnitInteraction2D : MonoBehaviour
{
    [Header("설정")]
    public float holdThreshold = 0.25f; // 이 시간보다 길게 누르면 드래그로 인식
    public LayerMask unitLayer; // 유닛만 클릭하기 위한 레이어 필터

    [Header("연결")]
    public GameObject statWindow; // 띄울 스탯창 UI

    private bool isPressed = false;
    private bool isDragging = false;
    private float pressTime = 0f;

    private Vector3 offset; // 마우스와 유닛 중심 간의 거리 차이
    private Camera mainCamera;
    private int originalSortingOrder; // 드래그 시 위로 띄우기 위한 변수
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. 마우스 눌렀을 때 (터치 시작)
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 위치를 2D 월드 좌표로 변환
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // 해당 위치에 있는 Collider2D를 감지 (Raycast)
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, unitLayer);

            if (hit.collider != null)
            {
                // 내가 맞았는지 확인
                if (hit.collider.gameObject == gameObject)
                {
                    isPressed = true;
                    isDragging = false;
                    pressTime = Time.time;

                    // 드래그 시 유닛이 마우스 중앙에 오지 않고 잡은 위치 유지
                    offset = transform.position - (Vector3)mousePos;
                }
            }
        }

        // 2. 누르고 있는 상태 (홀드 체크)
        if (isPressed && !isDragging)
        {
            if (Time.time - pressTime > holdThreshold)
            {
                StartDragging();
            }
        }

        // 3. 드래그 중 (이동)
        if (isDragging)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            // Z축은 유지하고 X, Y만 따라다니게 함
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
        }

        // 4. 마우스 뗐을 때
        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            isPressed = false;

            if (isDragging)
            {
                EndDragging();
            }
            else
            {
                // 드래그가 아니었으면 클릭으로 간주
                ToggleStatWindow();
            }
        }
    }

    void StartDragging()
    {
        isDragging = true;
        Debug.Log("드래그 시작");

        // 시각적 효과: 드래그 중인 유닛이 다른 유닛보다 위에 보이게 설정
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 100; // 아주 높은 값으로 설정해서 맨 위로

            // 살짝 커지는 효과 (TFT 느낌)
            transform.localScale = Vector3.one * 1.2f;
        }
    }

    void EndDragging()
    {
        isDragging = false;
        Debug.Log("배치 완료");

        // 시각적 효과 원상복구
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
            transform.localScale = Vector3.one;
        }

        // 여기에 '가장 가까운 타일로 이동' 코드 추가
    }

    void ToggleStatWindow()
    {
        Debug.Log("스탯창 토글");
        if (statWindow != null)
        {
            statWindow.SetActive(!statWindow.activeSelf);
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GridCellVisual : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector2Int gridPosition;
    private GridSystem gridSystem;

    public Vector2Int GridPosition => gridPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int x, int y, GridSystem grid)
    {
        gridPosition = new Vector2Int(x, y);
        gridSystem = grid;
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }

    // 선택적: 마우스 이벤트로도 처리 가능
    private void OnMouseDown()
    {
        if (gridSystem != null)
        {
            gridSystem.OnCellClicked?.Invoke(gridPosition);
        }
    }
}
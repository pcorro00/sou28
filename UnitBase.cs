using UnityEngine;

public class UnitBase : MonoBehaviour
{
    [Header("Unit Stats")]
    [SerializeField] protected float attackRange = 3f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackSpeed = 1f; // 초당 공격 횟수
    [SerializeField] protected int maxHealth = 100;

    protected int currentHealth;
    protected Vector2Int gridPosition;
    protected float lastAttackTime;

    public Vector2Int GridPosition => gridPosition;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void Initialize(Vector2Int pos)
    {
        gridPosition = pos;
        Debug.Log($"Unit initialized at {gridPosition}");
    }

    protected virtual void Update()
    {
        // 적 탐지 및 공격 로직은 나중에 구현
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"Unit at {gridPosition} died!");
        // 그리드에서 제거하는 로직 추가 필요
        Destroy(gameObject);
    }

    // 에디터에서 공격 범위 표시
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

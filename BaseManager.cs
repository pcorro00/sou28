using UnityEngine;

/// <summary>
/// 플레이어 기지 관리
/// </summary>
public class BaseManager : MonoBehaviour
{
    [Header("기지 설정")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Transform basePosition;    // 기지 위치

    private float currentHealth;

    // 이벤트
    public System.Action<float, float> OnHealthChanged; // (current, max)
    public System.Action OnBaseDestroyed;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // 기지 위치 자동 설정 (좌측)
        if (basePosition == null)
        {
            basePosition = transform;
            transform.position = new Vector3(-10, 0, 0);
        }

        Debug.Log($"Base initialized at {basePosition.position} with {maxHealth} HP");
    }

    // 데미지 받기
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Base took {damage} damage! HP: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            DestroyBase();
        }
    }

    // 회복
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log($"Base healed {amount}! HP: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 기지 파괴
    private void DestroyBase()
    {
        Debug.Log("=== BASE DESTROYED! GAME OVER ===");
        OnBaseDestroyed?.Invoke();

        // 게임 오버 처리
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    // 상태 조회
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public Vector3 GetPosition() => basePosition.position;

    // 기즈모: 기지 위치 표시
    private void OnDrawGizmos()
    {
        Vector3 pos = basePosition != null ? basePosition.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(pos, Vector3.one * 2f);

        if (Application.isPlaying)
        {
            // 체력에 따라 색상 변경
            float healthPercent = GetHealthPercentage();
            Gizmos.color = Color.Lerp(Color.red, Color.green, healthPercent);
            Gizmos.DrawCube(pos, Vector3.one * 1.5f);
        }
    }

  
}

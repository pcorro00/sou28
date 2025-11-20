using UnityEngine;

/// <summary>
/// 개별 적 컨트롤러
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("적 정보")]
    [SerializeField] private EnemyData enemyData;

    [Header("전투 설정 (EnemyData에서 자동 설정)")]
    private float attackRange;          // 공격 범위 (EnemyData에서)
    private float attackCooldown;       // 공격 쿨다운 (EnemyData에서)

    [Header("현재 상태")]
    private float currentHealth;
    private Vector3 targetPosition;     // 목표 지점 (기지)
    private bool isDead = false;

    [Header("특수 스탯")]
    [SerializeField] public float criticalChance = 0f;
    [SerializeField] public float criticalDamage = 1.2f;  
    [SerializeField] public float evasionChance = 0f;

    // 전투 관련
    private UnitStats currentTarget;                        // 현재 공격 대상
    private float lastAttackTime;                           // 마지막 공격 시간

    // 프로퍼티
    public EnemyData EnemyData => enemyData;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => enemyData != null ? enemyData.maxHealth : 100f;
    public bool IsDead => isDead;
    public float AttackDamage => enemyData != null ? enemyData.attackDamage : 10f;
    public float AttackRange => attackRange;
    public float Defense => enemyData != null ? enemyData.defense : 0f;


    public float CriticalChance => criticalChance;
    public float CriticalDamage => criticalDamage;
    public float EvasionChance => evasionChance;

    private void Awake()
    {
        if (enemyData != null)
        {
            currentHealth = enemyData.maxHealth;
            // 전투 스탯도 초기화
            attackRange = enemyData.attackRange;
            attackCooldown = enemyData.attackCooldown;
        }

        // Rigidbody2D 추가 (Physics 감지를 위해 필요)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic; // Kinematic으로
        rb.gravityScale = 0; // 중력 없음

        // 기존 Circle Collider 제거
        CircleCollider2D oldCircle = GetComponent<CircleCollider2D>();
        if (oldCircle != null)
        {
            Destroy(oldCircle);
            Debug.Log("Removed old Circle Collider");
        }

        // Capsule Collider 설정 (유닛이 감지할 수 있도록)
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        if (capsule == null)
        {
            capsule = gameObject.AddComponent<CapsuleCollider2D>();
            capsule.direction = CapsuleDirection2D.Vertical; // 세로 방향
            capsule.size = new Vector2(0.5f, 0.8f); // 캐릭터 크기에 맞게
        }

        capsule.isTrigger = true; // 트리거로 설정
    }

    // 적 초기화 (스폰 시 호출)
    public void Initialize(EnemyData data, Vector3 target)
    {
        enemyData = data;
        targetPosition = target;
        currentHealth = data.maxHealth;
        isDead = false;

        // 전투 스탯 설정 (EnemyData에서)
        attackRange = data.attackRange;
        attackCooldown = data.attackCooldown;
        criticalChance = data.criticalChance;
        criticalDamage = data.criticalDamage;  // 추가
        evasionChance = data.evasionChance;

        // 비주얼 설정
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.sprite != null)
        {
            sr.sprite = data.sprite;
            sr.color = data.color;
        }

        transform.localScale = Vector3.one * data.scale;

        Debug.Log($"{data.enemyName} spawned with {currentHealth} HP, ATK Range: {attackRange}, ATK CD: {attackCooldown}");
    }

    // EnemyController.cs의 Update 메서드를 이것으로 교체

    private void Update()
    {
        if (isDead) return;

        DetectUnits();

        // 타겟이 있으면
        if (currentTarget != null && !currentTarget.IsDead)
        {
            // 거리 체크
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distance > attackRange)
            {
                // 범위 밖: 타겟을 향해 이동
                Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
                transform.position += direction * enemyData.moveSpeed * Time.deltaTime;

                if (Time.frameCount % 30 == 0)
                {
                    Debug.Log($"[UPDATE] Moving towards target. Distance: {distance:F2}");
                }
            }
            else
            {
                // 범위 안: 공격
                AttackUnit();
            }
        }
        else
        {
            // 타겟 없음: 기지로 이동
            currentTarget = null;
            MoveTowardsTarget();
        }
    }

    // 목표 지점으로 이동
    private void MoveTowardsTarget()
    {
        if (enemyData == null) return;

        // 좌측으로 이동
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * enemyData.moveSpeed * Time.deltaTime;

        // 목표 지점 도달 확인 (기지에 도달)
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance < 0.5f)
        {
            ReachBase();
        }
    }

    // 기지 도달
    private void ReachBase()
    {
        Debug.Log($"{enemyData.enemyName} reached the base! Damage: {enemyData.attackDamage}");

        // 기지에 데미지 (나중에 구현)
        BaseManager baseManager = FindFirstObjectByType<BaseManager>();
        if (baseManager != null)
        {
            baseManager.TakeDamage(enemyData.attackDamage);
        }

        // 적 제거
        Die(false);
    }

    // 데미지 받기
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{enemyData.enemyName} took {damage} damage. HP: {currentHealth}/{MaxHealth}");

        // 데미지 이펙트 (간단한 색상 변화)
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die(true);
        }
    }

    // ========== 유닛 전투 시스템 ==========

    // 주변 유닛 감지
    private void DetectUnits()
    {
        // 공격 범위 내의 모든 콜라이더 검색
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        if (Time.frameCount % 60 == 0)  // 1초마다 로그
        {
            Debug.Log($"[ENEMY DETECT] {enemyData.enemyName} found {hits.Length} colliders, Range: {attackRange}");
        }

        // 현재 적의 Grid Row 가져오기
        GridSystem gridSystem = FindFirstObjectByType<GridSystem>();
        int myRow = -1;
        if (gridSystem != null)
        {
            Vector2Int myGridPos = gridSystem.WorldToGridPosition(transform.position);
            myRow = myGridPos.y;
        }

        float closestDistance = float.MaxValue;
        UnitStats closestUnit = null;

        foreach (Collider2D hit in hits)
        {
            // 자기 자신은 제외
            if (hit.gameObject == gameObject) continue;

            // UnitStats 컴포넌트가 있는지 확인
            UnitStats unit = hit.GetComponent<UnitStats>();
            if (unit != null && unit.CurrentHealth > 0 && !unit.IsDead)
            {
                // 같은 Row에 있는지 확인 (중요!)
                if (gridSystem != null)
                {
                    Vector2Int unitGridPos = gridSystem.WorldToGridPosition(unit.transform.position);
                    int unitRow = unitGridPos.y;

                    // 다른 Row면 무시
                    if (unitRow != myRow)
                    {
                        continue;
                    }
                }

                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestUnit = unit;

                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"<color=yellow>[ENEMY TARGET] {enemyData.enemyName} targeting {unit.CharacterName} at distance {distance:F2}</color>");
                    }
                }
            }
        }

        // 가장 가까운 유닛을 타겟으로 설정
        if (closestUnit != null && currentTarget != closestUnit)
        {
            currentTarget = closestUnit;
            Debug.Log($"<color=green>[ENEMY] {enemyData.enemyName} locked target: {currentTarget.CharacterName}</color>");
        }
        else if (closestUnit == null && currentTarget != null)
        {
            Debug.Log($"<color=gray>[ENEMY] {enemyData.enemyName} lost target</color>");
            currentTarget = null;
        }
    }

    // 유닛 공격
    private void AttackUnit()
    {
        Debug.Log($"[AttackUnit] Called! Target: {currentTarget?.CharacterName}");

        if (currentTarget == null)
        {
            Debug.Log("[AttackUnit] Target is null!");
            return;
        }

        float cooldownRemaining = attackCooldown - (Time.time - lastAttackTime);
        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log($"[AttackUnit] Cooldown! Remaining: {cooldownRemaining:F2}s");
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        Debug.Log($"[AttackUnit] Distance: {distance:F2}, AttackRange: {attackRange}");

        if (distance > attackRange)
        {
            Debug.LogWarning($"[AttackUnit] Out of range! {distance:F2} > {attackRange}");
            currentTarget = null;
            return;
        }

        // 여기까지 왔으면 공격!
        lastAttackTime = Time.time;
        currentTarget.TakeDamage(enemyData.attackDamage);

        Debug.Log($"<color=red>[ENEMY ATTACK] {enemyData.enemyName} attacked {currentTarget.CharacterName} for {enemyData.attackDamage} damage!</color>");

        StartCoroutine(AttackPunchEffect());
    }

    // 공격 이펙트
    private System.Collections.IEnumerator AttackPunchEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.1f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }

    // 데미지 받을 때 깜빡임
    private System.Collections.IEnumerator DamageFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    // 사망 처리
    private void Die(bool killedByPlayer)
    {
        if (isDead) return;

        isDead = true;

        if (killedByPlayer)
        {
            Debug.Log($"{enemyData.enemyName} killed! Reward: {enemyData.goldReward} gold");

            // 보상 지급
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.AddGold(enemyData.goldReward);
                gameManager.AddExperience(enemyData.expReward);
            }
        }

        // 제거
        Destroy(gameObject, 0.2f);
    }

    // 체력 바 표시용 (나중에 UI에서 사용)
    public float GetHealthPercentage()
    {
        return currentHealth / MaxHealth;
    }

    // 디버그용: 기즈모로 목표 지점 및 공격 범위 표시
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && !isDead)
        {
            // 목표 지점 (기지)
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.5f);

            // 공격 범위
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 현재 타겟
            if (currentTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    }
   
}
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 유닛 스탯 시스템 (마나 기반 전투)
/// </summary>
public class UnitStats : MonoBehaviour
{
    [Header("캐릭터 정보")]
    [SerializeField] private UnitType unitType;
    [SerializeField] private string characterName;

    [Header("기본 스탯")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float defense = 0f;

    [Header("마나 시스템")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegen = 20f;     // 초당 마나 회복

    [Header("특수 스탯")]
    [SerializeField] private float criticalChance = 0f;
    [SerializeField] private float criticalDamage = 1.2f;
    [SerializeField] private float evasionChance = 0f;
    [SerializeField] private float lifeSteal = 0f;
    [SerializeField] private float healthRegen = 0f;

    [Header("시너지 (Traits)")]
    [SerializeField] private UnitClass unitClass;
    [SerializeField] private UnitRace unitRace;

    [Header("전투 설정")]
    [SerializeField] private bool autoAttack = true;
    [SerializeField] private bool showDebugLogs = false;

    [Header("현재 상태")]
    private float currentHealth;
    private float currentMana = 0f;     // 시작 시 0
    private Vector2Int gridPosition;
    private EnemyController currentTarget;
    private List<EnemyController> enemiesInRange = new List<EnemyController>();

    // 시너지 버프 (TraitManager가 설정)
    [HideInInspector] public float traitHealthMultiplier = 1f;
    [HideInInspector] public float traitAttackMultiplier = 1f;
    [HideInInspector] public float traitDefenseBonus = 0f;
    [HideInInspector] public float traitManaRegenMultiplier = 1f;
    [HideInInspector] public float traitCritChanceBonus = 0f;

    // 프로퍼티
    public Vector2Int GridPosition => gridPosition;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth * traitHealthMultiplier;
    public float CurrentMana => currentMana;
    public float MaxMana => maxMana;
    public UnitType UnitType => unitType;
    public string CharacterName => characterName;
    public UnitClass UnitClass => unitClass;
    public UnitRace UnitRace => unitRace;
    public bool IsDead => currentHealth <= 0;
    public float CriticalDamage => criticalDamage;
    private void Awake()
    {
        currentHealth = MaxHealth;
        currentMana = 0f;
    }

    private void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[START] {characterName} started at {transform.position}");
        }
    }

    private void Update()
    {
        if (!autoAttack) return;

        // 마나 재생
        RegenerateMana();

        // 체력 재생
        RegenerateHealth();

        // 적 탐지
        DetectEnemies();

        // 공격 (마나가 가득 차면)
        AttackTarget();
    }

    public void Initialize(Vector2Int pos)
    {
        gridPosition = pos;
        Debug.Log($"{characterName} initialized at grid {gridPosition}");
    }

    public void InitializeFromUnit(UnitData unitData)
    {
        if (unitData == null) return;

        unitType = unitData.unitType;
        characterName = unitData.unitName;

        // 기본 스탯
        maxHealth = unitData.baseHealth;
        currentHealth = MaxHealth;
        attackDamage = unitData.baseAttackDamage;
        attackRange = unitData.baseAttackRange;
        defense = unitData.baseDefense;

        // 마나
        maxMana = unitData.baseMaxMana;
        manaRegen = unitData.baseManaRegen;
        currentMana = 0f;

        // 특수 스탯
        criticalChance = unitData.baseCriticalChance;
        criticalDamage = unitData.baseCriticalDamage;
        evasionChance = unitData.baseEvasionChance;
        lifeSteal = unitData.baseLifeSteal;
        healthRegen = unitData.baseHealthRegen;

        // 시너지
        unitClass = unitData.unitClass;
        unitRace = unitData.unitRace;

        Debug.Log($"Unit initialized: {unitData.unitName} ({unitData.unitClass}/{unitData.unitRace})");
    }

    /// <summary>
    /// 마나 재생
    /// </summary>
    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            float regenAmount = manaRegen * traitManaRegenMultiplier * Time.deltaTime;
            currentMana = Mathf.Min(currentMana + regenAmount, maxMana);
        }
    }

    /// <summary>
    /// 체력 재생
    /// </summary>
    private void RegenerateHealth()
    {
        if (healthRegen > 0 && currentHealth < MaxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + healthRegen * Time.deltaTime, MaxHealth);
        }
    }

    private void DetectEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        enemiesInRange.Clear();

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDead)
            {
                enemiesInRange.Add(enemy);
            }
        }

        if (enemiesInRange.Count > 0)
        {
            currentTarget = GetClosestEnemy();
        }
        else
        {
            currentTarget = null;
        }
    }

    private EnemyController GetClosestEnemy()
    {
        EnemyController closest = null;
        float minDistance = float.MaxValue;

        foreach (EnemyController enemy in enemiesInRange)
        {
            if (enemy == null || enemy.IsDead) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    private void AttackTarget()
    {
        if (currentTarget == null) return;

        // 마나가 가득 찼는지 확인
        if (currentMana < maxMana)
        {
            return; // 마나 부족
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distance > attackRange)
        {
            currentTarget = null;
            return;
        }

        // 공격!
        PerformAttack(currentTarget);

        // 마나 소모
        currentMana = 0f;
    }

    /// <summary>
    /// 실제 공격 실행
    /// </summary>
    private void PerformAttack(EnemyController target)
    {
        if (target == null) return;

        // 최종 공격력 (시너지 버프 적용)
        float finalDamage = attackDamage * traitAttackMultiplier;

        // 치명타 체크
        bool isCritical = false;
        float totalCritChance = criticalChance + traitCritChanceBonus;
        if (Random.Range(0f, 100f) < totalCritChance)
        {
            isCritical = true;
            finalDamage *= criticalDamage; // 치명타 배수 데미지
        }

        // 공격
        target.TakeDamage(finalDamage);

        // 흡혈
        if (lifeSteal > 0)
        {
            float healAmount = finalDamage * (lifeSteal / 100f);
            Heal(healAmount);
        }

        if (showDebugLogs || isCritical)
        {
            string critText = isCritical ? " CRITICAL!" : "";
            Debug.Log($"<color=red>[ATTACK] {characterName} attacked {target.EnemyData.enemyName} for {finalDamage:F1} damage!{critText}</color>");
        }

        StartCoroutine(AttackPunchEffect());
    }

    private System.Collections.IEnumerator AttackPunchEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }

    public void TakeDamage(float damage)
    {
        // 회피 체크
        if (Random.Range(0f, 100f) < evasionChance)
        {
            Debug.Log($"{characterName} evaded attack!");
            return;
        }

        // 방어력 적용
        float totalDefense = defense + traitDefenseBonus;
        float damageReduction = totalDefense / (totalDefense + 100f);
        float finalDamage = damage * (1f - damageReduction);

        currentHealth -= finalDamage;

        if (showDebugLogs)
        {
            Debug.Log($"{characterName} took {finalDamage:F1} damage (Defense: {totalDefense}). HP: {currentHealth:F1}/{MaxHealth:F1}");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
    }

    private void Die()
    {
        Debug.Log($"{characterName} died!");

        GridSystem gridSystem = FindFirstObjectByType<GridSystem>();
        if (gridSystem != null)
        {
            gridSystem.RemoveUnit(gridPosition);
        }

        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (Application.isPlaying && currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
    private void OnMouseDown()
    {
        if (UnitInfoUI.Instance != null)
        {
            UnitInfoUI.Instance.ShowUnitInfo(this);
        }
    }
}

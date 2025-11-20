using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitInfoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Stat Icons - Row 1")]
    [SerializeField] private StatIconUI healthIcon;
    [SerializeField] private StatIconUI attackIcon;
    [SerializeField] private StatIconUI rangeIcon;
    [SerializeField] private StatIconUI defenseIcon;
    [SerializeField] private StatIconUI maxManaIcon;
    [SerializeField] private StatIconUI manaRegenIcon;

    [Header("Stat Icons - Row 2")]
    [SerializeField] private StatIconUI critChanceIcon;
    [SerializeField] private StatIconUI critDamageIcon;
    [SerializeField] private StatIconUI evasionIcon;
    [SerializeField] private StatIconUI lifeStealIcon;
    [SerializeField] private StatIconUI healthRegenIcon;

    [Header("Icon Sprites")]
    [SerializeField] private Sprite heartIcon;
    [SerializeField] private Sprite swordIcon;
    [SerializeField] private Sprite bowIcon;
    [SerializeField] private Sprite shieldIcon;
    [SerializeField] private Sprite manaIcon;
    [SerializeField] private Sprite manaRegenIconSprite;
    [SerializeField] private Sprite critIcon;
    [SerializeField] private Sprite critDmgIcon;
    [SerializeField] private Sprite dodgeIcon;
    [SerializeField] private Sprite vampireIcon;
    [SerializeField] private Sprite regenIcon;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    private UnitStats currentUnit;
    private EnemyController currentEnemy;

    public static UnitInfoUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("UnitInfoUI Start called!");

        // 패널 숨김 (간단하게!)
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        Debug.Log("Unit Info UI initialized");
    }

    public void ShowUnitInfo(UnitStats unit)
    {
        Debug.Log($"ShowUnitInfo called for: {(unit != null ? unit.CharacterName : "NULL")}");

        if (unit == null) return;

        currentUnit = unit;
        currentEnemy = null;

        // 패널 활성화 - 이 부분이 꼭 있어야 합니다!
        if (infoPanel != null)
        {
            Debug.Log($"Before SetActive: {infoPanel.activeSelf}");
            infoPanel.SetActive(true);
            Debug.Log($"After SetActive: {infoPanel.activeSelf}");
        }
        else
        {
            Debug.LogError("infoPanel is NULL in ShowUnitInfo!");
        }

        UpdateUnitInfo();
    }

    public void ShowEnemyInfo(EnemyController enemy)
    {
        Debug.Log($"ShowEnemyInfo called for: {(enemy != null ? enemy.EnemyData.enemyName : "NULL")}");

        if (enemy == null) return;

        currentEnemy = enemy;
        currentUnit = null;

        // 패널 활성화 - 이 부분 추가!
        if (infoPanel != null)
        {
            Debug.Log($"Before SetActive: {infoPanel.activeSelf}");
            infoPanel.SetActive(true);
            Debug.Log($"After SetActive: {infoPanel.activeSelf}");
        }
        else
        {
            Debug.LogError("infoPanel is NULL in ShowEnemyInfo!");
        }

        UpdateEnemyInfo();
    }


    private void UpdateUnitInfo()
    {
        if (currentUnit == null) return;

        // 이름
        if (nameText != null)
        {
            nameText.text = currentUnit.CharacterName;
        }

        // Row 1 - MaxHealth 계산 수정
        if (healthIcon != null)
        {
            float maxHp = currentUnit.MaxHealth;  // 이미 시너지 적용된 값
            healthIcon.Setup(heartIcon, $"{currentUnit.CurrentHealth:F0}/{maxHp:F0}");
        }

        if (attackIcon != null)
        {
            float finalAttack = currentUnit.attackDamage * currentUnit.traitAttackMultiplier;
            attackIcon.Setup(swordIcon, $"{finalAttack:F1}");
        }

        if (rangeIcon != null)
        {
            rangeIcon.Setup(bowIcon, $"{currentUnit.attackRange:F1}");
        }

        if (defenseIcon != null)
        {
            float finalDefense = currentUnit.defense + currentUnit.traitDefenseBonus;
            defenseIcon.Setup(shieldIcon, $"{finalDefense:F1}");
        }

        if (maxManaIcon != null)
        {
            maxManaIcon.Setup(manaIcon, $"{currentUnit.MaxMana:F0}");
        }

        if (manaRegenIcon != null)
        {
            float finalManaRegen = currentUnit.manaRegen * currentUnit.traitManaRegenMultiplier;
            manaRegenIcon.Setup(manaRegenIconSprite, $"{finalManaRegen:F1}/s");
        }

        // Row 2
        if (critChanceIcon != null)
        {
            float finalCritChance = currentUnit.criticalChance + currentUnit.traitCritChanceBonus;
            critChanceIcon.Setup(critIcon, $"{finalCritChance:F0}%");
        }

        if (critDamageIcon != null)
        {
            critDamageIcon.Setup(critDmgIcon, $"{currentUnit.CriticalDamage:F1}x");
        }

        if (evasionIcon != null)
        {
            evasionIcon.Setup(dodgeIcon, $"{currentUnit.evasionChance:F0}%");
        }

        if (lifeStealIcon != null)
        {
            lifeStealIcon.Setup(vampireIcon, $"{currentUnit.lifeSteal:F0}%");
        }

        if (healthRegenIcon != null)
        {
            healthRegenIcon.Setup(regenIcon, $"{currentUnit.healthRegen:F1}/s");
        }
    }

    private void UpdateEnemyInfo()
    {
        if (currentEnemy == null) return;

        // 이름
        if (nameText != null)
        {
            nameText.text = currentEnemy.EnemyData.enemyName;
        }

        // 설명
        if (descriptionText != null)
        {
            descriptionText.text = currentEnemy.EnemyData.description;
        }

        // Row 1
        healthIcon?.Setup(heartIcon, $"{currentEnemy.CurrentHealth:F0}/{currentEnemy.MaxHealth:F0}");
        attackIcon?.Setup(swordIcon, $"{currentEnemy.AttackDamage:F1}");
        rangeIcon?.Setup(bowIcon, $"{currentEnemy.AttackRange:F1}");
        defenseIcon?.Setup(shieldIcon, $"{currentEnemy.Defense:F1}");
        maxManaIcon?.Setup(manaIcon, "-");
        manaRegenIcon?.Setup(manaRegenIconSprite, "-");

        // Row 2
        critChanceIcon?.Setup(critIcon, $"{currentEnemy.CriticalChance:F0}%");
        critDamageIcon?.Setup(critDmgIcon, $"{currentEnemy.CriticalDamage:F1}x");
        evasionIcon?.Setup(dodgeIcon, $"{currentEnemy.EvasionChance:F0}%");
        lifeStealIcon?.Setup(vampireIcon, "-");
        healthRegenIcon?.Setup(regenIcon, "-");
    }
    private void Update()
    {
        // 실시간 업데이트
        if (currentUnit != null)
        {
            UpdateUnitInfo();

            // ESC나 패널 밖 클릭으로 닫기
            if (Input.GetKeyDown(KeyCode.Escape) ||
                (Input.GetMouseButtonDown(0) && !IsPointerOverPanel()))
            {
                HideInfo();
            }
        }
        else if (currentEnemy != null)
        {
            UpdateEnemyInfo();

            // ESC나 패널 밖 클릭으로 닫기
            if (Input.GetKeyDown(KeyCode.Escape) ||
                (Input.GetMouseButtonDown(0) && !IsPointerOverPanel()))
            {
                HideInfo();
            }
        }
    }

    private bool IsPointerOverPanel()
    {
        if (infoPanel == null || !infoPanel.activeSelf) return false;

        RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
        if (panelRect == null) return false;

        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect,
            Input.mousePosition,
            null,
            out localMousePosition
        );

        return panelRect.rect.Contains(localMousePosition);
    }
    public void HideInfo()
    {
        // 패널 숨김 (간단하게!)
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        currentUnit = null;
        currentEnemy = null;
    }
}
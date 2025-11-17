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
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideInfo);
        }

        Debug.Log("Unit Info UI initialized");
    }

    public void ShowUnitInfo(UnitStats unit)
    {
        if (unit == null) return;

        currentUnit = unit;
        currentEnemy = null;

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        UpdateUnitInfo();
    }

    public void ShowEnemyInfo(EnemyController enemy)
    {
        if (enemy == null) return;

        currentEnemy = enemy;
        currentUnit = null;

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
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

        // 설명 (UnitData에서 가져와야 함 - 일단 생략)
        if (descriptionText != null)
        {
            descriptionText.text = "유닛 설명";
        }

        // Row 1
        healthIcon?.Setup(heartIcon, $"{currentUnit.CurrentHealth:F0}/{currentUnit.MaxHealth:F0}");

        float finalAttack = currentUnit.AttackDamage * currentUnit.traitAttackMultiplier;
        attackIcon?.Setup(swordIcon, $"{finalAttack:F1}");

        rangeIcon?.Setup(bowIcon, $"{currentUnit.AttackRange:F1}");

        float finalDefense = currentUnit.Defense + currentUnit.traitDefenseBonus;
        defenseIcon?.Setup(shieldIcon, $"{finalDefense:F1}");

        maxManaIcon?.Setup(manaIcon, $"{currentUnit.MaxMana:F0}");

        float finalManaRegen = currentUnit.ManaRegen * currentUnit.traitManaRegenMultiplier;
        manaRegenIcon?.Setup(manaRegenIconSprite, $"{finalManaRegen:F1}/s");

        // Row 2
        float finalCritChance = currentUnit.CriticalChance + currentUnit.traitCritChanceBonus;
        critChanceIcon?.Setup(critIcon, $"{finalCritChance:F0}%");

        critDamageIcon?.Setup(critDmgIcon, $"{currentUnit.CriticalDamage:F1}x");

        evasionIcon?.Setup(dodgeIcon, $"{currentUnit.EvasionChance:F0}%");

        lifeStealIcon?.Setup(vampireIcon, $"{currentUnit.LifeSteal:F0}%");

        healthRegenIcon?.Setup(regenIcon, $"{currentUnit.HealthRegen:F1}/s");
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

    public void HideInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        currentUnit = null;
        currentEnemy = null;
    }
}
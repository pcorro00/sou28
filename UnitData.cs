using UnityEngine;

/// <summary>
/// 유닛 등급 (Tier)
/// </summary>
public enum UnitTier
{
    Tier1 = 1,  // 기본 유닛
    Tier2 = 2,  // 1차 진화
    Tier3 = 3,  // 2차 진화
    Tier4 = 4,  // 3차 진화
    Tier5 = 5   // 최종 진화
}

/// <summary>
/// 유닛 타입
/// </summary>
public enum UnitType
{
    // Tier 1 (기본)
    Warrior,        // 전사
    Archer,         // 궁수
    Mage,           // 마법사
    
    // Tier 2
    Knight,         // 기사 (전사+전사)
    Ranger,         // 레인저 (전사+궁수)
    MasterArcher,   // 명궁 (궁수+궁수)
    Wizard,         // 마법사 (마법사+마법사)
    BattleMage,     // 배틀메이지 (전사+마법사)
    
    // Tier 3
    Paladin,        // 성기사 (기사+기사)
    Sniper,         // 스나이퍼 (명궁+명궁)
    Archmage,       // 대마법사 (마법사+마법사)
    
    // Tier 4
    DragonKnight,   // 드래곤 나이트 (성기사+성기사)
    
    // 기타
    Unknown
}

/// <summary>
/// 유닛 데이터 (ScriptableObject)
/// </summary>
[CreateAssetMenu(fileName = "New Unit", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("기본 정보")]
    public UnitType unitType;           // 유닛 타입
    public string unitName;             // 유닛 이름
    public UnitTier tier;               // 유닛 등급
    
    [Header("시너지 (Traits)")]
    public UnitClass unitClass;         // 직업 (전사/마법사/궁수)
    public UnitRace unitRace;           // 종족 (인간/엘프/드워프)
    
    [Header("비주얼")]
    public Sprite icon;                 // 아이콘
    public GameObject unitPrefab;       // 유닛 프리팹
    public Color displayColor = Color.white;
    
    [Header("기본 스탯")]
    public float baseHealth = 100f;             // 체력
    public float baseAttackDamage = 10f;        // 공격력
    public float baseAttackRange = 3f;          // 공격 사정거리
    public float baseDefense = 0f;              // 방어력 (받는 데미지 감소)
    
    [Header("마나 시스템")]
    public float baseMaxMana = 100f;            // 최대 마나
    public float baseManaRegen = 20f;           // 마나 재생 속도 (초당)
    
    [Header("특수 스탯")]
    [Range(0f, 100f)]
    public float baseCriticalChance = 0f;       // 치명타 확률 (%)
    [Range(0f, 100f)]
    public float baseEvasionChance = 0f;        // 회피 확률 (%)
    [Range(0f, 100f)]
    public float baseLifeSteal = 0f;            // 체력 흡혈 (%)
    public float baseHealthRegen = 0f;          // 체력 재생 속도 (초당)
    
    [Header("설명")]
    [TextArea(3, 5)]
    public string description;
    
    [Header("가격 (상점)")]
    public int goldCost = 50;
    
    /// <summary>
    /// 유닛의 전체 이름 (디버그용)
    /// </summary>
    public string GetFullName()
    {
        return $"{unitName} (Tier {(int)tier})";
    }
    
    /// <summary>
    /// 시너지 정보 텍스트
    /// </summary>
    public string GetTraitInfo()
    {
        return $"{unitClass} / {unitRace}";
    }
}

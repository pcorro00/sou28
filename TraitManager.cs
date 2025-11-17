using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TraitInfo
{
    public int currentCount;
    public List<int> thresholds = new List<int>();
    public int activeLevel = 0;
}

public class TraitManager : MonoBehaviour
{
    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    // 특성별 카운트
    private Dictionary<UnitClass, TraitInfo> classTraits = new Dictionary<UnitClass, TraitInfo>();
    private Dictionary<UnitRace, TraitInfo> raceTraits = new Dictionary<UnitRace, TraitInfo>();

    // 배치된 유닛들 (죽어도 유지)
    private List<UnitStats> allDeployedUnits = new List<UnitStats>();

    // Singleton
    public static TraitManager Instance { get; private set; }

    public System.Action OnTraitsChanged;

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

        InitializeTraits();
    }

    private void Start()
    {
        Debug.Log("Trait Manager initialized");
        InvokeRepeating(nameof(UpdateAllTraits), 0.5f, 0.5f);
    }

    private void InitializeTraits()
    {
        // 직업 시너지 설정
        classTraits[UnitClass.Warrior] = new TraitInfo
        {
            thresholds = new List<int> { 3, 6, 9 }
        };
        classTraits[UnitClass.Mage] = new TraitInfo
        {
            thresholds = new List<int> { 2, 4, 6 }
        };
        classTraits[UnitClass.Archer] = new TraitInfo
        {
            thresholds = new List<int> { 2, 4, 6 }
        };

        // 종족 시너지 설정
        raceTraits[UnitRace.Human] = new TraitInfo
        {
            thresholds = new List<int> { 2, 4 }
        };
        raceTraits[UnitRace.Elf] = new TraitInfo
        {
            thresholds = new List<int> { 2, 4 }
        };
        raceTraits[UnitRace.Dwarf] = new TraitInfo
        {
            thresholds = new List<int> { 2, 4 }
        };
    }

    /// <summary>
    /// 유닛이 배치될 때 호출
    /// </summary>
    public void RegisterUnit(UnitStats unit)
    {
        if (unit == null) return;

        if (!allDeployedUnits.Contains(unit))
        {
            allDeployedUnits.Add(unit);
            Debug.Log($"Unit registered: {unit.CharacterName} ({unit.UnitType})");
            UpdateAllTraits();
        }
    }

    /// <summary>
    /// 유닛이 제거될 때 호출 (판매 등)
    /// </summary>
    public void UnregisterUnit(UnitStats unit)
    {
        if (allDeployedUnits.Contains(unit))
        {
            allDeployedUnits.Remove(unit);
            Debug.Log($"Unit unregistered: {unit.CharacterName}");
            UpdateAllTraits();
        }
    }

    /// <summary>
    /// 모든 시너지 업데이트
    /// </summary>
    public void UpdateAllTraits()
    {
        // 카운트 계산
        CountTraits();

        // 버프 적용
        ApplyTraitBuffs();

        // UI 업데이트
        OnTraitsChanged?.Invoke();
    }

    /// <summary>
    /// 특성 카운트 (중복 유닛은 1개로만)
    /// </summary>
    private void CountTraits()
    {
        // 초기화
        foreach (var trait in classTraits.Values)
        {
            trait.currentCount = 0;
            trait.activeLevel = 0;
        }
        foreach (var trait in raceTraits.Values)
        {
            trait.currentCount = 0;
            trait.activeLevel = 0;
        }

        // 중복 제거를 위한 유닛 타입별 그룹화
        HashSet<UnitType> uniqueUnitTypes = new HashSet<UnitType>();

        Debug.Log($"=== Counting Traits: Total {allDeployedUnits.Count} units ===");

        foreach (UnitStats unit in allDeployedUnits)
        {
            if (unit == null) continue;

            // 이미 계산된 유닛 타입은 스킵 (중복 제거!)
            if (uniqueUnitTypes.Contains(unit.UnitType))
            {
                if (showDebugLogs)
                    Debug.Log($"Skipping duplicate: {unit.UnitType}");
                continue;
            }

            uniqueUnitTypes.Add(unit.UnitType);

            // 직업 카운트
            if (unit.UnitClass != UnitClass.None && classTraits.ContainsKey(unit.UnitClass))
            {
                classTraits[unit.UnitClass].currentCount++;
                if (showDebugLogs)
                    Debug.Log($"{unit.UnitClass} unique count: {classTraits[unit.UnitClass].currentCount}");
            }

            // 종족 카운트
            if (unit.UnitRace != UnitRace.None && raceTraits.ContainsKey(unit.UnitRace))
            {
                raceTraits[unit.UnitRace].currentCount++;
            }
        }

        // 활성 레벨 계산
        CalculateActiveLevels();
    }

    private void CalculateActiveLevels()
    {
        // 직업
        foreach (var kvp in classTraits)
        {
            TraitInfo info = kvp.Value;
            int previousLevel = info.activeLevel;
            info.activeLevel = 0;

            for (int i = info.thresholds.Count - 1; i >= 0; i--)
            {
                if (info.currentCount >= info.thresholds[i])
                {
                    info.activeLevel = i + 1;
                    break;
                }
            }

            if (info.activeLevel > previousLevel && info.activeLevel > 0)
            {
                Debug.Log($"<color=yellow>🔥 {kvp.Key} 시너지 활성! (Level {info.activeLevel}) - {info.currentCount}개의 고유 유닛</color>");
            }
        }

        // 종족
        foreach (var kvp in raceTraits)
        {
            TraitInfo info = kvp.Value;
            int previousLevel = info.activeLevel;
            info.activeLevel = 0;

            for (int i = info.thresholds.Count - 1; i >= 0; i--)
            {
                if (info.currentCount >= info.thresholds[i])
                {
                    info.activeLevel = i + 1;
                    break;
                }
            }

            if (info.activeLevel > previousLevel && info.activeLevel > 0)
            {
                Debug.Log($"<color=cyan>🔥 {kvp.Key} 시너지 활성! (Level {info.activeLevel}) - {info.currentCount}개의 고유 유닛</color>");
            }
        }
    }

    private void ApplyTraitBuffs()
    {
        // 살아있는 유닛에게만 버프 적용
        foreach (UnitStats unit in allDeployedUnits)
        {
            if (unit == null || unit.IsDead) continue;

            // 버프 초기화
            unit.traitHealthMultiplier = 1f;
            unit.traitAttackMultiplier = 1f;
            unit.traitDefenseBonus = 0f;
            unit.traitManaRegenMultiplier = 1f;
            unit.traitCritChanceBonus = 0f;

            // 직업 버프
            ApplyClassBuff(unit);

            // 종족 버프
            ApplyRaceBuff(unit);
        }
    }

    private void ApplyClassBuff(UnitStats unit)
    {
        UnitClass unitClass = unit.UnitClass;
        if (unitClass == UnitClass.None || !classTraits.ContainsKey(unitClass))
            return;

        int level = classTraits[unitClass].activeLevel;
        if (level == 0) return;

        switch (unitClass)
        {
            case UnitClass.Warrior:
                if (level == 1) unit.traitHealthMultiplier = 1.3f;
                else if (level == 2) unit.traitHealthMultiplier = 1.7f;
                else if (level == 3) unit.traitHealthMultiplier = 2.5f;
                break;

            case UnitClass.Mage:
                if (level == 1) unit.traitAttackMultiplier = 1.25f;
                else if (level == 2) unit.traitAttackMultiplier = 1.6f;
                else if (level == 3) unit.traitAttackMultiplier = 2.2f;
                break;

            case UnitClass.Archer:
                if (level == 1) unit.traitManaRegenMultiplier = 1.3f;
                else if (level == 2) unit.traitManaRegenMultiplier = 1.7f;
                else if (level == 3) unit.traitManaRegenMultiplier = 2.5f;
                break;
        }
    }

    private void ApplyRaceBuff(UnitStats unit)
    {
        UnitRace unitRace = unit.UnitRace;
        if (unitRace == UnitRace.None || !raceTraits.ContainsKey(unitRace))
            return;

        int level = raceTraits[unitRace].activeLevel;
        if (level == 0) return;

        switch (unitRace)
        {
            case UnitRace.Human:
                if (level == 1) unit.traitDefenseBonus = 10f;
                else if (level == 2) unit.traitDefenseBonus = 25f;
                break;

            case UnitRace.Elf:
                if (level == 1) unit.traitCritChanceBonus = 10f;
                else if (level == 2) unit.traitCritChanceBonus = 20f;
                break;

            case UnitRace.Dwarf:
                if (level == 1)
                {
                    unit.traitHealthMultiplier *= 1.15f;
                    unit.traitDefenseBonus += 5f;
                }
                else if (level == 2)
                {
                    unit.traitHealthMultiplier *= 1.3f;
                    unit.traitDefenseBonus += 15f;
                }
                break;
        }
    }

    public TraitInfo GetClassTrait(UnitClass unitClass)
    {
        if (classTraits.ContainsKey(unitClass))
            return classTraits[unitClass];
        return null;
    }

    public TraitInfo GetRaceTrait(UnitRace unitRace)
    {
        if (raceTraits.ContainsKey(unitRace))
            return raceTraits[unitRace];
        return null;
    }
}
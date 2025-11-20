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

    private Dictionary<UnitClass, TraitInfo> classTraits = new Dictionary<UnitClass, TraitInfo>();
    private Dictionary<UnitRace, TraitInfo> raceTraits = new Dictionary<UnitRace, TraitInfo>();

    // 변경: UnitStats가 아닌 UnitData 리스트로 관리
    private List<UnitData> deployedUnitData = new List<UnitData>();

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
    public void UnregisterDeployedUnit(UnitData unitData)
    {
        if (unitData != null && deployedUnitData.Contains(unitData))
        {
            deployedUnitData.Remove(unitData);
            Debug.Log($"Unit unregistered from traits: {unitData.unitName}");
            UpdateAllTraits();
        }
    }
    private void Start()
    {
        Debug.Log("Trait Manager initialized");

        // 5초마다 시너지 업데이트 (활성화 메시지 출력)
        InvokeRepeating("UpdateAllTraits", 5f, 5f);
    }


    private void InitializeTraits()
    {
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

    // 새로 추가: 유닛 배치 시 호출
    public void RegisterDeployedUnit(UnitData unitData)
    {
        if (unitData != null)
        {
            deployedUnitData.Add(unitData);
            Debug.Log($"Unit registered for traits: {unitData.unitName}");
            UpdateAllTraits();
        }
    }

    // 새로 추가: 라운드 초기화 시 호출
    public void ClearDeployedUnits()
    {
        deployedUnitData.Clear();
        Debug.Log("Deployed units cleared for new round");
        UpdateAllTraits();
    }

    // 수정: FindDeployedUnits 호출 제거
    public void UpdateAllTraits()
    {
        CountTraits();
        ApplyTraitBuffs();
        OnTraitsChanged?.Invoke();
    }

    // 수정: deployedUnitData 사용
    private void CountTraits()
    {
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

        // 중복 제거: 같은 UnitType은 한 번만 카운트
        HashSet<UnitType> countedTypes = new HashSet<UnitType>();

        foreach (UnitData unit in deployedUnitData)
        {
            // 이미 카운트된 타입이면 스킵
            if (countedTypes.Contains(unit.unitType))
                continue;

            countedTypes.Add(unit.unitType);

            // 직업 카운트
            if (unit.unitClass != UnitClass.None && classTraits.ContainsKey(unit.unitClass))
            {
                classTraits[unit.unitClass].currentCount++;
            }

            // 종족 카운트
            if (unit.unitRace != UnitRace.None && raceTraits.ContainsKey(unit.unitRace))
            {
                raceTraits[unit.unitRace].currentCount++;
            }
        }

        CalculateActiveLevels();
    }

    private void CalculateActiveLevels()
    {
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
                Debug.Log($"[TRAIT] {kvp.Key} 시너지 활성! (Level {info.activeLevel})");
            }
        }

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
                Debug.Log($"[TRAIT] {kvp.Key} 시너지 활성! (Level {info.activeLevel})");
            }
        }
    }

    // 수정: 살아있는 유닛만 찾아서 버프 적용
    private void ApplyTraitBuffs()
    {
        UnitStats[] aliveUnits = FindObjectsByType<UnitStats>(FindObjectsSortMode.None);

        foreach (UnitStats unit in aliveUnits)
        {
            if (unit == null || unit.IsDead) continue;

            unit.traitHealthMultiplier = 1f;
            unit.traitAttackMultiplier = 1f;
            unit.traitDefenseBonus = 0f;
            unit.traitManaRegenMultiplier = 1f;
            unit.traitCritChanceBonus = 0f;

            ApplyClassBuff(unit);
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

    public List<string> GetActiveTraits()
    {
        List<string> active = new List<string>();

        foreach (var kvp in classTraits)
        {
            if (kvp.Value.activeLevel > 0)
            {
                active.Add($"{kvp.Key} ({kvp.Value.currentCount})");
            }
        }

        foreach (var kvp in raceTraits)
        {
            if (kvp.Value.activeLevel > 0)
            {
                active.Add($"{kvp.Key} ({kvp.Value.currentCount})");
            }
        }

        return active;
    }
}
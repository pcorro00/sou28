using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 시너지 정보
/// </summary>
[System.Serializable]
public class TraitInfo
{
    public int currentCount;
    public List<int> thresholds = new List<int>();
    public int activeLevel = 0; // 0 = 비활성, 1/2/3 = 레벨
}

/// <summary>
/// 시너지 시스템 관리
/// </summary>
public class TraitManager : MonoBehaviour
{
    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;
    
    // 특성별 카운트
    private Dictionary<UnitClass, TraitInfo> classTraits = new Dictionary<UnitClass, TraitInfo>();
    private Dictionary<UnitRace, TraitInfo> raceTraits = new Dictionary<UnitRace, TraitInfo>();
    
    // 배치된 유닛들
    private List<UnitStats> deployedUnits = new List<UnitStats>();
    
    // Singleton
    public static TraitManager Instance { get; private set; }
    
    // 이벤트
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
        
        // 1초마다 업데이트 (최적화)
        InvokeRepeating(nameof(UpdateAllTraits), 0.5f, 0.5f);
    }
    
    /// <summary>
    /// 특성 초기화
    /// </summary>
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
    /// 모든 시너지 업데이트
    /// </summary>
    public void UpdateAllTraits()
    {
        // 배치된 유닛 찾기
        FindDeployedUnits();
        
        // 카운트 계산
        CountTraits();
        
        // 버프 적용
        ApplyTraitBuffs();
        
        // UI 업데이트
        OnTraitsChanged?.Invoke();
    }
    
    /// <summary>
    /// 배치된 유닛 찾기
    /// </summary>
    private void FindDeployedUnits()
    {
        deployedUnits.Clear();
        
        // 씬의 모든 UnitStats 찾기
        UnitStats[] allUnits = FindObjectsByType<UnitStats>(FindObjectsSortMode.None);
        
        foreach (UnitStats unit in allUnits)
        {
            if (unit != null && !unit.IsDead)
            {
                deployedUnits.Add(unit);
            }
        }
    }
    
    /// <summary>
    /// 특성 카운트
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
        
        // 카운트
        foreach (UnitStats unit in deployedUnits)
        {
            // 직업 카운트
            if (unit.UnitClass != UnitClass.None && classTraits.ContainsKey(unit.UnitClass))
            {
                classTraits[unit.UnitClass].currentCount++;
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
    
    /// <summary>
    /// 활성 레벨 계산
    /// </summary>
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
            
            // 새로 활성화되었을 때 로그
            if (info.activeLevel > previousLevel && info.activeLevel > 0)
            {
                Debug.Log($"<color=yellow>🔥 {kvp.Key} 시너지 활성! (Level {info.activeLevel})</color>");
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
            
            // 새로 활성화되었을 때 로그
            if (info.activeLevel > previousLevel && info.activeLevel > 0)
            {
                Debug.Log($"<color=cyan>🔥 {kvp.Key} 시너지 활성! (Level {info.activeLevel})</color>");
            }
        }
    }
    
    /// <summary>
    /// 버프 적용
    /// </summary>
    private void ApplyTraitBuffs()
    {
        foreach (UnitStats unit in deployedUnits)
        {
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
    
    /// <summary>
    /// 직업 버프 적용
    /// </summary>
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
                // 전사: 체력 버프
                if (level == 1) unit.traitHealthMultiplier = 1.3f;  // +30%
                else if (level == 2) unit.traitHealthMultiplier = 1.7f;  // +70%
                else if (level == 3) unit.traitHealthMultiplier = 2.5f;  // +150%
                break;
                
            case UnitClass.Mage:
                // 마법사: 공격력 버프
                if (level == 1) unit.traitAttackMultiplier = 1.25f;  // +25%
                else if (level == 2) unit.traitAttackMultiplier = 1.6f;  // +60%
                else if (level == 3) unit.traitAttackMultiplier = 2.2f;  // +120%
                break;
                
            case UnitClass.Archer:
                // 궁수: 마나 재생 버프
                if (level == 1) unit.traitManaRegenMultiplier = 1.3f;  // +30%
                else if (level == 2) unit.traitManaRegenMultiplier = 1.7f;  // +70%
                else if (level == 3) unit.traitManaRegenMultiplier = 2.5f;  // +150%
                break;
        }
    }
    
    /// <summary>
    /// 종족 버프 적용
    /// </summary>
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
                // 인간: 방어력 버프
                if (level == 1) unit.traitDefenseBonus = 10f;
                else if (level == 2) unit.traitDefenseBonus = 25f;
                break;
                
            case UnitRace.Elf:
                // 엘프: 치명타 확률 버프
                if (level == 1) unit.traitCritChanceBonus = 10f;
                else if (level == 2) unit.traitCritChanceBonus = 20f;
                break;
                
            case UnitRace.Dwarf:
                // 드워프: 체력 + 방어력
                if (level == 1)
                {
                    unit.traitHealthMultiplier *= 1.15f;  // +15%
                    unit.traitDefenseBonus += 5f;
                }
                else if (level == 2)
                {
                    unit.traitHealthMultiplier *= 1.3f;  // +30%
                    unit.traitDefenseBonus += 15f;
                }
                break;
        }
    }
    
    /// <summary>
    /// 특정 직업의 시너지 정보 가져오기
    /// </summary>
    public TraitInfo GetClassTrait(UnitClass unitClass)
    {
        if (classTraits.ContainsKey(unitClass))
            return classTraits[unitClass];
        return null;
    }
    
    /// <summary>
    /// 특정 종족의 시너지 정보 가져오기
    /// </summary>
    public TraitInfo GetRaceTrait(UnitRace unitRace)
    {
        if (raceTraits.ContainsKey(unitRace))
            return raceTraits[unitRace];
        return null;
    }
    
    /// <summary>
    /// 활성화된 모든 시너지 가져오기
    /// </summary>
    public List<string> GetActiveTraits()
    {
        List<string> active = new List<string>();
        
        // 직업
        foreach (var kvp in classTraits)
        {
            if (kvp.Value.activeLevel > 0)
            {
                active.Add($"{kvp.Key} ({kvp.Value.currentCount})");
            }
        }
        
        // 종족
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 유닛 업그레이드 레시피
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Game/Upgrade Recipe")]
public class UpgradeRecipe : ScriptableObject
{
    [Header("레시피 정보")]
    public string recipeName;               // 레시피 이름 (예: "기사 업그레이드")
    
    [Header("필요 유닛 (2~5개)")]
    public List<UnitType> requiredUnits = new List<UnitType>();  // 필요한 유닛들
    
    [Header("결과")]
    public UnitType resultUnit;             // 결과 유닛
    public UnitData resultUnitData;         // 결과 유닛 데이터
    
    [Header("설명")]
    [TextArea(2, 3)]
    public string description;
    
    /// <summary>
    /// 레시피가 유효한지 확인 (2~5개)
    /// </summary>
    public bool IsValid()
    {
        return requiredUnits.Count >= 2 && requiredUnits.Count <= 5;
    }
    
    /// <summary>
    /// 주어진 유닛들이 이 레시피와 일치하는지 확인
    /// </summary>
    public bool CheckRecipe(List<UnitType> units)
    {
        if (units.Count != requiredUnits.Count)
            return false;
        
        // 유닛 개수 세기
        Dictionary<UnitType, int> requiredCounts = new Dictionary<UnitType, int>();
        Dictionary<UnitType, int> providedCounts = new Dictionary<UnitType, int>();
        
        // 필요한 유닛 개수 세기
        foreach (UnitType unit in requiredUnits)
        {
            if (!requiredCounts.ContainsKey(unit))
                requiredCounts[unit] = 0;
            requiredCounts[unit]++;
        }
        
        // 제공된 유닛 개수 세기
        foreach (UnitType unit in units)
        {
            if (!providedCounts.ContainsKey(unit))
                providedCounts[unit] = 0;
            providedCounts[unit]++;
        }
        
        // 개수 비교
        if (requiredCounts.Count != providedCounts.Count)
            return false;
        
        foreach (var pair in requiredCounts)
        {
            if (!providedCounts.ContainsKey(pair.Key) || providedCounts[pair.Key] != pair.Value)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 레시피 설명 텍스트
    /// </summary>
    public string GetRecipeText()
    {
        string units = string.Join(" + ", requiredUnits.Select(u => u.ToString()));
        return $"{units} = {resultUnit}";
    }
}

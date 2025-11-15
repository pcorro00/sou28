using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 유닛 인벤토리 시스템
/// </summary>
public class UnitInventory : MonoBehaviour
{
    [Header("보유 유닛")]
    [SerializeField] private List<UnitData> units = new List<UnitData>();
    
    // 이벤트
    public System.Action OnInventoryChanged;
    
    // Singleton
    public static UnitInventory Instance { get; private set; }
    
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
        Debug.Log($"Unit Inventory initialized with {units.Count} units");
    }
    
    /// <summary>
    /// 유닛 추가
    /// </summary>
    public bool AddUnit(UnitData unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("Cannot add null unit!");
            return false;
        }
        
        units.Add(unit);
        Debug.Log($"Added {unit.GetFullName()} to inventory");
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    /// <summary>
    /// 유닛 제거
    /// </summary>
    public bool RemoveUnit(UnitData unit)
    {
        if (unit == null) return false;
        
        bool removed = units.Remove(unit);
        
        if (removed)
        {
            Debug.Log($"Removed {unit.GetFullName()} from inventory");
            OnInventoryChanged?.Invoke();
        }
        
        return removed;
    }
    
    /// <summary>
    /// 여러 유닛 제거
    /// </summary>
    public bool RemoveUnits(List<UnitData> unitsToRemove)
    {
        bool allRemoved = true;
        
        foreach (UnitData unit in unitsToRemove)
        {
            if (!RemoveUnit(unit))
            {
                allRemoved = false;
            }
        }
        
        return allRemoved;
    }
    
    /// <summary>
    /// 모든 유닛 가져오기
    /// </summary>
    public List<UnitData> GetAllUnits()
    {
        return new List<UnitData>(units);
    }
    
    /// <summary>
    /// Tier별로 유닛 가져오기
    /// </summary>
    public List<UnitData> GetUnitsByTier(UnitTier tier)
    {
        return units.Where(u => u.tier == tier).ToList();
    }
    
    /// <summary>
    /// 특정 타입의 유닛 개수
    /// </summary>
    public int GetUnitCount(UnitType type)
    {
        return units.Count(u => u.unitType == type);
    }
    
    /// <summary>
    /// 특정 타입의 유닛 가져오기
    /// </summary>
    public List<UnitData> GetUnitsByType(UnitType type)
    {
        return units.Where(u => u.unitType == type).ToList();
    }
    
    /// <summary>
    /// 인벤토리 비우기
    /// </summary>
    public void ClearInventory()
    {
        units.Clear();
        Debug.Log("Inventory cleared");
        OnInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// 인벤토리 정보 출력 (디버그)
    /// </summary>
    public void PrintInventory()
    {
        Debug.Log("=== Unit Inventory ===");
        
        for (int tier = 1; tier <= 5; tier++)
        {
            var tierUnits = GetUnitsByTier((UnitTier)tier);
            if (tierUnits.Count > 0)
            {
                Debug.Log($"Tier {tier}: {string.Join(", ", tierUnits.Select(u => u.unitName))}");
            }
        }
        
        Debug.Log($"Total units: {units.Count}");
    }
}

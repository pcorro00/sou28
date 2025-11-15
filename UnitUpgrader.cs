using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 조합 결과
/// </summary>
public class UpgradeResult
{
    public bool isSuccess;              // 성공 여부
    public UpgradeRecipe recipe;        // 사용된 레시피
    public UnitData resultUnit;         // 결과 유닛
    public string message;              // 결과 메시지
    public bool isNewDiscovery;         // 새로운 발견인지
    
    public UpgradeResult(bool success, UpgradeRecipe recipe, UnitData result, string msg, bool newDiscovery = false)
    {
        isSuccess = success;
        this.recipe = recipe;
        resultUnit = result;
        message = msg;
        isNewDiscovery = newDiscovery;
    }
}

/// <summary>
/// 유닛 업그레이드/조합 시스템
/// </summary>
public class UnitUpgrader : MonoBehaviour
{
    [Header("레시피 목록")]
    [SerializeField] private List<UpgradeRecipe> recipes = new List<UpgradeRecipe>();
    
    [Header("발견된 레시피")]
    private HashSet<UnitType> discoveredRecipes = new HashSet<UnitType>();
    
    // 이벤트
    public System.Action<UpgradeResult> OnUpgradeComplete;
    public System.Action<UpgradeRecipe> OnNewRecipeDiscovered;
    
    // Singleton
    public static UnitUpgrader Instance { get; private set; }
    
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
        Debug.Log($"Unit Upgrader initialized with {recipes.Count} recipes");
    }
    
    /// <summary>
    /// 유닛들을 조합
    /// </summary>
    public UpgradeResult UpgradeUnits(List<UnitData> units)
    {
        // 유효성 검사
        if (units == null || units.Count < 2 || units.Count > 5)
        {
            return new UpgradeResult(false, null, null, "유닛 개수가 잘못되었습니다! (2~5개 필요)");
        }
        
        // 유닛 타입 추출
        List<UnitType> unitTypes = units.Select(u => u.unitType).ToList();
        
        // 레시피 찾기
        UpgradeRecipe matchedRecipe = FindRecipe(unitTypes);
        
        if (matchedRecipe == null)
        {
            string unitList = string.Join(" + ", unitTypes);
            Debug.LogWarning($"No recipe found for {unitList}");
            return new UpgradeResult(false, null, null, "조합 레시피를 찾을 수 없습니다!");
        }
        
        // 결과 유닛 데이터
        UnitData resultUnit = matchedRecipe.resultUnitData;
        
        if (resultUnit == null)
        {
            Debug.LogError("Recipe has no result unit data!");
            return new UpgradeResult(false, matchedRecipe, null, "레시피 데이터 오류!");
        }
        
        // 새로운 발견인지 확인
        bool isNew = !discoveredRecipes.Contains(matchedRecipe.resultUnit);
        if (isNew)
        {
            discoveredRecipes.Add(matchedRecipe.resultUnit);
            OnNewRecipeDiscovered?.Invoke(matchedRecipe);
            Debug.Log($"NEW RECIPE DISCOVERED: {resultUnit.unitName}!");
        }
        
        string message = isNew
            ? $"NEW! {resultUnit.unitName} 발견!"
            : $"{resultUnit.unitName} 생성!";
        
        Debug.Log($"=== 조합 성공 ===");
        Debug.Log($"재료: {string.Join(" + ", unitTypes)}");
        Debug.Log($"결과: {resultUnit.unitName}");
        
        UpgradeResult result = new UpgradeResult(true, matchedRecipe, resultUnit, message, isNew);
        OnUpgradeComplete?.Invoke(result);
        
        return result;
    }
    
    /// <summary>
    /// 레시피 찾기
    /// </summary>
    private UpgradeRecipe FindRecipe(List<UnitType> units)
    {
        return recipes.FirstOrDefault(r => r.CheckRecipe(units));
    }
    
    /// <summary>
    /// 레시피 미리보기
    /// </summary>
    public UpgradeRecipe PreviewUpgrade(List<UnitType> units)
    {
        return FindRecipe(units);
    }
    
    /// <summary>
    /// 레시피가 발견되었는지 확인
    /// </summary>
    public bool IsRecipeDiscovered(UnitType type)
    {
        return discoveredRecipes.Contains(type);
    }
    
    /// <summary>
    /// 발견된 레시피 수
    /// </summary>
    public int GetDiscoveredCount()
    {
        return discoveredRecipes.Count;
    }
    
    /// <summary>
    /// 전체 레시피 수
    /// </summary>
    public int GetTotalRecipeCount()
    {
        return recipes.Count;
    }
    
    /// <summary>
    /// 모든 레시피 가져오기
    /// </summary>
    public List<UpgradeRecipe> GetAllRecipes()
    {
        return new List<UpgradeRecipe>(recipes);
    }
    
    /// <summary>
    /// 발견된 레시피만 가져오기
    /// </summary>
    public List<UpgradeRecipe> GetDiscoveredRecipes()
    {
        return recipes.Where(r => IsRecipeDiscovered(r.resultUnit)).ToList();
    }
    
    /// <summary>
    /// 레시피 추가 (런타임)
    /// </summary>
    public void AddRecipe(UpgradeRecipe recipe)
    {
        if (recipe != null && !recipes.Contains(recipe))
        {
            recipes.Add(recipe);
            Debug.Log($"Recipe added: {recipe.recipeName}");
        }
    }
}

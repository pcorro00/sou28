using UnityEngine;

// 더미 클래스 - 기존 코드 호환용
public enum CharacterType
{
    HumanWarrior,
    HumanCrossbowman,
    ElfArcher,
    ElfMage,
    DwarfWarrior,
    OrcBerserker,
    DragonKnight,
    Paladin,
    Necromancer,
    DemonLord,
    Unknown
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Game/Recipe Data (Old)")]
public class RecipeData : ScriptableObject
{
    public CharacterType characterType;
    public string characterName;
    
  
    public GameObject unitPrefab;
    public Sprite characterIcon;
    
    public float baseHealth = 100f;
    public float baseAttackDamage = 10f;
    public float baseAttackSpeed = 1f;
    public float baseAttackRange = 3f;
    
    public string description;
    
 
}

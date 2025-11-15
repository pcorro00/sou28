using UnityEngine;

/// <summary>
/// 적 타입
/// </summary>
public enum EnemyType
{
    Slime,      // 슬라임 (약함)
    Goblin,     // 고블린 (보통)
    Orc,        // 오크 (강함)
    Dragon,     // 드래곤 (보스)
    Skeleton,   // 스켈레톤
    Zombie      // 좀비
}

/// <summary>
/// 적 데이터
/// </summary>
[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;            // 적 이름
    public EnemyType enemyType;         // 적 타입

    [Header("스탯")]
    public float maxHealth = 50f;       // 체력
    public float moveSpeed = 1f;        // 이동 속도
    public float attackDamage = 10f;    // 기지 공격 데미지

    [Header("전투 스탯")]
    public float attackRange = 1.5f;    // 공격 범위
    public float attackCooldown = 1f;   // 공격 쿨다운 (초)

    [Header("보상")]
    public int goldReward = 10;         // 처치 시 골드
    public int expReward = 5;           // 처치 시 경험치

    [Header("비주얼")]
    public Sprite sprite;               // 적 스프라이트
    public Color color = Color.red;     // 색상
    public float scale = 1f;            // 크기

    [Header("설명")]
    [TextArea(2, 3)]
    public string description;
}
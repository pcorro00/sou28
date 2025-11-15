using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체 관리
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("게임 상태")]
    private int currentGold = 100;          // 시작 골드
    private int currentExperience = 0;      // 경험치
    private int playerLevel = 1;            // 레벨
    private int expToNextLevel = 100;       // 다음 레벨까지 필요 경험치
    private bool isGameOver = false;

    // 싱글톤
    public static GameManager Instance { get; private set; }

    // 이벤트
    public System.Action<int> OnGoldChanged;
    public System.Action<int, int, int> OnExpChanged;   // (current, max, level)
    public System.Action<int> OnLevelUp;
    public System.Action OnGameOver;
    public System.Action OnGameWin;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log("=== Game Started ===");
        Debug.Log($"Gold: {currentGold}, Level: {playerLevel}");
    }

    // 골드 추가
    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"Gold +{amount} = {currentGold}");
        OnGoldChanged?.Invoke(currentGold);
    }

    // 골드 사용
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            Debug.Log($"Gold -{amount} = {currentGold}");
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }

        Debug.LogWarning($"Not enough gold! Need: {amount}, Have: {currentGold}");
        return false;
    }

    // 경험치 추가
    public void AddExperience(int amount)
    {
        currentExperience += amount;
        Debug.Log($"EXP +{amount} = {currentExperience}/{expToNextLevel}");
        OnExpChanged?.Invoke(currentExperience, expToNextLevel, playerLevel);

        // 레벨업 체크
        while (currentExperience >= expToNextLevel)
        {
            LevelUp();
        }
    }

    // 레벨업
    private void LevelUp()
    {
        currentExperience -= expToNextLevel;
        playerLevel++;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f); // 20% 증가

        Debug.Log($"=== LEVEL UP! Level {playerLevel} ===");
        OnLevelUp?.Invoke(playerLevel);

     
        ShowPartSelection();
    }

    // 게임 오버
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("=== GAME OVER ===");
        OnGameOver?.Invoke();

        Time.timeScale = 0; // 게임 정지
    }

    // 게임 승리
    public void GameWin()
    {
        Debug.Log("=== YOU WIN! ===");
        OnGameWin?.Invoke();

        Time.timeScale = 0;
    }

    // 게임 재시작
    public void RestartGame()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    // 상태 조회
    public int GetGold() => currentGold;
    public int GetExperience() => currentExperience;
    public int GetLevel() => playerLevel;
    public bool IsGameOver() => isGameOver;

    // 디버그용: 테스트 단축키
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(100);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            AddExperience(50);
        }
    }
    // 파츠 선택창 표시
    [Header("테스트용 유닛")]
    public List<UnitData> testUnits;  // Inspector에서 설정
    private void ShowPartSelection()
    {
        // 임시: 랜덤 유닛 자동 추가
        UnitInventory inventory = UnitInventory.Instance;
        if (inventory != null)
        {
            // 나중에 만들 testUnits 배열에서 랜덤 선택
            // 지금은 그냥 넘어가기
            Debug.Log("레벨업! (유닛 선택 UI는 나중에 구현)");
        }
    }

}
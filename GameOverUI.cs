using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 오버 UI 관리
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;

    // 시스템 참조
    private GameManager gameManager;
    private EnemySpawner enemySpawner;

    // 통계
    private int totalKills = 0;
    private int totalGold = 0;

    // Singleton
    public static GameOverUI Instance { get; private set; }

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
        // 시스템 찾기
        gameManager = GameManager.Instance;
        enemySpawner = FindFirstObjectByType<EnemySpawner>();

        // 이벤트 구독
        if (gameManager != null)
        {
            gameManager.OnGameOver += ShowGameOver;
            gameManager.OnGameWin += ShowVictory;
            gameManager.OnGoldChanged += OnGoldChanged;
        }

        if (enemySpawner != null)
        {
            enemySpawner.OnEnemyDied += OnEnemyKilled;
        }

        // 버튼 이벤트
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        // 초기 상태
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        Debug.Log("GameOver UI initialized");
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameOver -= ShowGameOver;
            gameManager.OnGameWin -= ShowVictory;
            gameManager.OnGoldChanged -= OnGoldChanged;
        }

        if (enemySpawner != null)
        {
            enemySpawner.OnEnemyDied -= OnEnemyKilled;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
    }

    /// <summary>
    /// 골드 변경 추적
    /// </summary>
    private void OnGoldChanged(int gold)
    {
        // 총 획득 골드 계산 (현재 골드 - 시작 골드)
        // 간단하게 현재 골드를 표시
        totalGold = gold;
    }

    /// <summary>
    /// 적 처치 시
    /// </summary>
    private void OnEnemyKilled(EnemyController enemy)
    {
        totalKills++;
    }

    /// <summary>
    /// 게임 오버 표시 (패배)
    /// </summary>
    private void ShowGameOver()
    {
        Debug.Log("=== Showing Game Over Screen ===");

        if (titleText != null)
        {
            titleText.text = " GAME OVER ";
            titleText.color = new Color(1f, 0.3f, 0.3f); // 빨간색
        }

        if (subtitleText != null)
        {
            subtitleText.text = "Your base was destroyed";
        }

        UpdateStats();
        ShowPanel();
    }

    /// <summary>
    /// 승리 표시
    /// </summary>
    private void ShowVictory()
    {
        Debug.Log("=== Showing Victory Screen ===");

        if (titleText != null)
        {
            titleText.text = " VICTORY! ";
            titleText.color = new Color(1f, 0.84f, 0f); // 노란색
        }

        if (subtitleText != null)
        {
            subtitleText.text = "You have won!";
        }

        UpdateStats();
        ShowPanel();
    }

    /// <summary>
    /// 통계 업데이트
    /// </summary>
    private void UpdateStats()
    {
        int wave = enemySpawner != null ? enemySpawner.GetCurrentWave() : 0;

        if (waveText != null)
        {
            waveText.text = $" Wave Reached: {wave}";
        }

        if (killsText != null)
        {
            killsText.text = $" Enemies Killed: {totalKills}";
        }

        if (goldText != null)
        {
            goldText.text = $" Gold Earned: {totalGold}";
        }
    }

    /// <summary>
    /// 패널 표시
    /// </summary>
    private void ShowPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 게임 정지 (이미 GameManager에서 했지만 확실하게)
        Time.timeScale = 0;
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    private void RestartGame()
    {
        Debug.Log("Restarting game...");

        // 게임 속도 복구
        Time.timeScale = 1;

        // 씬 재로드
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    /// <summary>
    /// 테스트용: 강제 게임 오버
    /// </summary>
    private void Update()
    {
        // 테스트: O키로 게임 오버
        if (Input.GetKeyDown(KeyCode.O))
        {
            ShowGameOver();
        }

        // 테스트: V키로 승리
        if (Input.GetKeyDown(KeyCode.V))
        {
            ShowVictory();
        }
    }
}
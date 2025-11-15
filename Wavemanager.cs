using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 웨이브 시작 버튼 관리
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startWaveButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Systems")]
    private EnemySpawner enemySpawner;

    // Singleton
    public static WaveManager Instance { get; private set; }

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
        // EnemySpawner 찾기
        enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (enemySpawner == null)
        {
            Debug.LogError("EnemySpawner not found!");
            return;
        }

        // 버튼 이벤트 연결
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(OnStartWaveButtonClicked);
        }

        // EnemySpawner 이벤트 구독
        enemySpawner.OnWaveStart += OnWaveStarted;
        enemySpawner.OnWaveComplete += OnWaveCompleted;

        // 초기 상태
        UpdateButtonState();

        Debug.Log("Wave Manager initialized");
    }

    private void OnDestroy()
    {
        if (enemySpawner != null)
        {
            enemySpawner.OnWaveStart -= OnWaveStarted;
            enemySpawner.OnWaveComplete -= OnWaveCompleted;
        }

        if (startWaveButton != null)
        {
            startWaveButton.onClick.RemoveListener(OnStartWaveButtonClicked);
        }
    }

    /// <summary>
    /// 웨이브 시작 버튼 클릭
    /// </summary>
    private void OnStartWaveButtonClicked()
    {
        if (enemySpawner != null)
        {
            enemySpawner.StartNextWave();
            Debug.Log("Wave start button clicked!");
        }
    }

    /// <summary>
    /// 웨이브 시작 시
    /// </summary>
    private void OnWaveStarted(int wave)
    {
        UpdateButtonState();
    }

    /// <summary>
    /// 웨이브 완료 시
    /// </summary>
    private void OnWaveCompleted(int wave)
    {
        UpdateButtonState();
    }

    /// <summary>
    /// 버튼 상태 업데이트
    /// </summary>
    private void UpdateButtonState()
    {
        if (enemySpawner == null || startWaveButton == null) return;

        bool isSpawning = enemySpawner.IsSpawning();
        int enemiesAlive = enemySpawner.GetEnemiesAlive();

        // 스폰 중이거나 적이 살아있으면 버튼 비활성화
        bool canStartWave = !isSpawning && enemiesAlive == 0;

        startWaveButton.interactable = canStartWave;

        // 버튼 텍스트 업데이트
        UpdateButtonText(canStartWave);
    }

    /// <summary>
    /// 버튼 텍스트 업데이트
    /// </summary>
    private void UpdateButtonText(bool canStart)
    {
        if (buttonText == null) return;

        if (canStart)
        {
            int nextWave = enemySpawner.GetCurrentWave() + 1;
            buttonText.text = $" 웨이브 {nextWave} 시작";
        }
        else
        {
            if (enemySpawner.IsSpawning())
            {
                buttonText.text = "스폰 중...";
            }
            else
            {
                int enemiesAlive = enemySpawner.GetEnemiesAlive();
                buttonText.text = $"남은 적 ({enemiesAlive})";
            }
        }
    }

    private void Update()
    {
        // 실시간 버튼 상태 업데이트 (적 수가 바뀔 때마다)
        UpdateButtonState();

        // 테스트: R키로도 가능 (나중에 제거 가능)
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnStartWaveButtonClicked();
        }
    }
}
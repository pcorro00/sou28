using UnityEngine;
using TMPro;

/// <summary>
/// HUD (골드, 레벨, 체력 등) 관리
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI waveText;

    // 시스템 참조
    private GameManager gameManager;
    private BaseManager baseManager;
    private EnemySpawner enemySpawner;

    // Singleton
    public static HUDManager Instance { get; private set; }

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
        baseManager = FindFirstObjectByType<BaseManager>();
        enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        // 이벤트 구독
        gameManager.OnGoldChanged += UpdateGold;
        gameManager.OnLevelUp += UpdateLevel;
        gameManager.OnExpChanged += UpdateLevel;

        if (baseManager != null)
        {
            baseManager.OnHealthChanged += UpdateHealth;
        }

        if (enemySpawner != null)
        {
            enemySpawner.OnWaveStart += UpdateWave;
        }

        // 초기 표시
        UpdateGold(gameManager.GetGold());
        UpdateLevel(gameManager.GetLevel());

        if (baseManager != null)
        {
            UpdateHealth(baseManager.GetCurrentHealth(), baseManager.GetMaxHealth());
        }

        UpdateWave(enemySpawner != null ? enemySpawner.GetCurrentWave() : 0);

        Debug.Log("HUD Manager initialized");
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (gameManager != null)
        {
            gameManager.OnGoldChanged -= UpdateGold;
            gameManager.OnLevelUp -= UpdateLevel;
            gameManager.OnExpChanged -= UpdateLevel;
        }

        if (baseManager != null)
        {
            baseManager.OnHealthChanged -= UpdateHealth;
        }

        if (enemySpawner != null)
        {
            enemySpawner.OnWaveStart -= UpdateWave;
        }
    }

    /// <summary>
    /// 골드 표시 업데이트
    /// </summary>
    private void UpdateGold(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"💰 Gold: {gold}";
        }
    }

    /// <summary>
    /// 레벨 표시 업데이트
    /// </summary>
    private void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"⭐ Level: {level}";
        }
    }

    /// <summary>
    /// 레벨 표시 업데이트 (경험치 변경 시)
    /// </summary>
    private void UpdateLevel(int currentExp, int maxExp, int level)
    {
        UpdateLevel(level);
    }

    /// <summary>
    /// 체력 표시 업데이트
    /// </summary>
    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"❤️ HP: {currentHealth:F0}/{maxHealth:F0}";

            // 체력에 따라 색상 변경
            float healthPercent = currentHealth / maxHealth;

            if (healthPercent > 0.5f)
            {
                healthText.color = Color.white;
            }
            else if (healthPercent > 0.25f)
            {
                healthText.color = new Color(1f, 0.8f, 0f); // 주황색
            }
            else
            {
                healthText.color = Color.red;
            }
        }
    }

    /// <summary>
    /// 웨이브 표시 업데이트
    /// </summary>
    private void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            if (enemySpawner != null)
            {
                int enemiesAlive = enemySpawner.GetEnemiesAlive();
                waveText.text = $"🌊 Wave {wave} - {enemiesAlive} enemies";
            }
            else
            {
                waveText.text = $"🌊 Wave {wave}";
            }
        }
    }

    /// <summary>
    /// 수동 전체 업데이트
    /// </summary>
    public void RefreshAll()
    {
        if (gameManager != null)
        {
            UpdateGold(gameManager.GetGold());
            UpdateLevel(gameManager.GetLevel());
        }

        if (baseManager != null)
        {
            UpdateHealth(baseManager.GetCurrentHealth(), baseManager.GetMaxHealth());
        }

        if (enemySpawner != null)
        {
            UpdateWave(enemySpawner.GetCurrentWave());
        }
    }

    private void Update()
    {
        // 웨이브 정보는 매 프레임 업데이트 (적 수가 바뀌므로)
        if (enemySpawner != null && waveText != null)
        {
            int wave = enemySpawner.GetCurrentWave();
            int enemiesAlive = enemySpawner.GetEnemiesAlive();
            waveText.text = $"🌊 Wave {wave} - {enemiesAlive} enemies";
        }
    }
}
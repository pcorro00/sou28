using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 적 스폰 관리자
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private GameObject enemyPrefab;        // 적 프리팹
    [SerializeField] private Transform spawnPoint;          // 스폰 위치 (우측)
    [SerializeField] private Transform targetPoint;         // 목표 위치 (기지, 좌측)
    [SerializeField] private float spawnRangeY = 2f;        // Y축 스폰 범위

    [Header("그리드 연동")]
    [SerializeField] private GridSystem gridSystem;         // 그리드 참조
    [SerializeField] private bool useGridBounds = true;     // 그리드 범위 사용

    [Header("웨이브 설정")]
    [SerializeField] private float spawnInterval = 2f;      // 스폰 간격
    [SerializeField] private int enemiesPerWave = 5;        // 웨이브당 적 수
    [SerializeField] private float waveCooldown = 10f;      // 웨이브 사이 대기시간

    [Header("적 데이터")]
    [SerializeField] private List<EnemyData> enemyDataList = new List<EnemyData>();

    [Header("현재 상태")]
    private int currentWave = 0;
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    private List<EnemyController> activeEnemies = new List<EnemyController>();

    // 이벤트
    public System.Action<int> OnWaveStart;      // 웨이브 시작
    public System.Action<int> OnWaveComplete;   // 웨이브 완료
    public System.Action<EnemyController> OnEnemySpawned;   // 적 스폰
    public System.Action<EnemyController> OnEnemyDied;      // 적 사망

    private void Start()
    {
        // 그리드 시스템 찾기
        if (gridSystem == null)
        {
            gridSystem = FindFirstObjectByType<GridSystem>();
        }

        // 자동으로 스폰/타겟 포인트 설정
        if (spawnPoint == null)
        {
            GameObject spawn = new GameObject("SpawnPoint");
            spawn.transform.position = new Vector3(10, 2.5f, 0); // 우측 중앙
            spawnPoint = spawn.transform;
        }

        if (targetPoint == null)
        {
            GameObject target = new GameObject("TargetPoint");
            target.transform.position = new Vector3(-10, 2.5f, 0); // 좌측 중앙
            targetPoint = target.transform;
        }

        // 그리드 범위에 맞춰 스폰 범위 조정
        if (useGridBounds && gridSystem != null)
        {
            // GridSystem의 높이를 가져와서 스폰 범위 설정
            // 그리드가 5칸이면 0~5, 중앙은 2.5
            spawnRangeY = 2f; // 그리드 높이의 절반 정도
        }

        Debug.Log("Enemy Spawner Ready!");
        Debug.Log($"Spawn: {spawnPoint.position}, Target: {targetPoint.position}");
        Debug.Log($"Spawn Range Y: ±{spawnRangeY}");
    }

    private void Update()
    {
        // 테스트: W 키로 웨이브 시작
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartNextWave();
        }

        // 적 리스트 정리 (죽은 적 제거)
        activeEnemies.RemoveAll(e => e == null || e.IsDead);
        enemiesAlive = activeEnemies.Count;
    }

    // 다음 웨이브 시작
    public void StartNextWave()
    {
        if (isSpawning)
        {
            Debug.LogWarning("Wave already in progress!");
            return;
        }

        currentWave++;
        enemiesSpawned = 0;

        Debug.Log($"=== Wave {currentWave} Start! ===");
        Debug.Log($"Enemies: {enemiesPerWave}, Interval: {spawnInterval}s");

        OnWaveStart?.Invoke(currentWave);
        StartCoroutine(SpawnWave());
    }

    // 웨이브 스폰 코루틴
    private IEnumerator SpawnWave()
    {
        isSpawning = true;

        // 웨이브당 적 수만큼 스폰
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            enemiesSpawned++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"Wave {currentWave} spawn complete. Waiting for enemies to be cleared...");

        // 모든 적이 제거될 때까지 대기
        yield return new WaitUntil(() => enemiesAlive == 0);

        Debug.Log($"=== Wave {currentWave} Complete! ===");
        OnWaveComplete?.Invoke(currentWave);

        // 다음 웨이브 대기
        Debug.Log($"Next wave in {waveCooldown} seconds... (Press W to start now)");
        yield return new WaitForSeconds(waveCooldown);

        // 자동으로 다음 웨이브 (선택사항)
        // StartNextWave();
    }

    // 적 스폰
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned!");
            return;
        }

        Vector3 spawnPos;
        Vector3 targetPos;

        if (useGridBounds && gridSystem != null)
        {
            // 그리드의 랜덤한 줄(Row)에 스폰
            int randomRow = Random.Range(0, 8); // 그리드 높이 (0~4)

            // 그리드 좌표를 월드 좌표로 변환
            // 스폰: 그리드 오른쪽 끝 + 1칸 (화면 밖)
            Vector3 spawnGridPos = gridSystem.GridToWorldPosition(14, randomRow); // 그리드 너비+1
            spawnPos = spawnGridPos;

            // 타겟: 그리드 왼쪽 끝 -1칸 (기지)
            Vector3 targetGridPos = gridSystem.GridToWorldPosition(-1, randomRow); // 같은 줄
            targetPos = targetGridPos;

            Debug.Log($"Enemy spawn row: {randomRow}, World Y: {spawnPos.y}");
        }
        else
        {
            // 그리드 없을 때 기본 동작
            float randomY = Random.Range(-spawnRangeY, spawnRangeY);
            spawnPos = spawnPoint.position + new Vector3(0, randomY, 0);
            targetPos = targetPoint.position;
            targetPos.y = spawnPos.y;
        }

        // 적 생성
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        enemyObj.name = $"Enemy_{currentWave}_{enemiesSpawned}";

        // 적 초기화
        EnemyController enemy = enemyObj.GetComponent<EnemyController>();
        if (enemy != null)
        {
            EnemyData data = GetRandomEnemyData();
            enemy.Initialize(data, targetPos);

            activeEnemies.Add(enemy);
            OnEnemySpawned?.Invoke(enemy);
        }

        Debug.Log($"Enemy spawned at {spawnPos}, target: {targetPos}");
    }

    // 랜덤 적 데이터 가져오기
    private EnemyData GetRandomEnemyData()
    {
        if (enemyDataList.Count == 0)
        {
            Debug.LogError("No enemy data in list!");
            return null;
        }

        // 웨이브가 높아질수록 강한 적 등장 확률 증가 (나중에 구현)
        int index = Random.Range(0, enemyDataList.Count);
        return enemyDataList[index];
    }

    // 적 수동 제거 (테스트용)
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
        Debug.Log("All enemies cleared!");
    }

    // 현재 상태 정보
    public int GetCurrentWave() => currentWave;
    public int GetEnemiesAlive() => enemiesAlive;
    public bool IsSpawning() => isSpawning;

    // 기즈모: 스폰/타겟 지점 표시
    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 1f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.down * 2f);
        }

        if (targetPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetPoint.position, 1f);
            Gizmos.DrawLine(targetPoint.position, targetPoint.position + Vector3.down * 2f);
        }

        if (spawnPoint != null && targetPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPoint.position, targetPoint.position);
        }
    }
}
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("參考物件")]
    public GameObject enemyPrefab;
    public GameObject fuelPrefab;
    public GameObject player;

    [Header("生成設定")]
    public float minY = -2f;
    public float maxY = 2f;
    public float maxRandomGap = 3f;

    [Header("Retry 模式專用設定 (新增)")]
    public int retryWaveCount = 8;    // 一口氣生成的數量 
    public float retryStartDistance = 5f; // 第一個敵人距離畫面右邊多遠 (原本10，改成5讓他快點出現)
    public float retryGap = 4f;       // 每個敵人之間的間隔

    [Header("平衡性保底設定 (新增)")]
    public int maxConsecutiveEnemies = 6; // 連續出現敵人的最大上限
    private int consecutiveEnemyCount = 0; // 內部計數器：目前連續生了幾個敵人

    [Range(0, 100)]
    public int fuelChance = 30;

    private float playerWidth;

    void Start()
    {
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            playerWidth = (playerCollider != null) ? playerCollider.bounds.size.x : 1f;
        }

        consecutiveEnemyCount = 0;

        // 如果是 Retry，依然可以執行提早生成 (這不受狀態影響，因為是一次性的)
        if (GameManager.isRetry) SpawnEarlyWave();
    }

    public void SpawnEarlyWave()
    {
        // 從畫面右邊緣附近開始 (數值越小越快看到)
        float currentX = retryStartDistance;

        // 使用 retryWaveCount 來決定生成幾個
        // 建議設為 8~10 個，這樣足以覆蓋到背景捲動觸發下一波
        for (int i = 0; i < retryWaveCount; i++)
        {
            // 隨機高度
            float spawnY = Random.Range(minY, maxY);
            Vector3 pos = new Vector3(currentX, spawnY, 0);

            // 決定生成什麼
            GameObject prefabToSpawn = (Random.Range(0, 100) < fuelChance) ? fuelPrefab : enemyPrefab;
            Instantiate(prefabToSpawn, pos, Quaternion.identity);

            // 計算下一個位置：固定間隔 + 一點點隨機 (讓它不要太死板)
            // 這裡確保每個物件之間有足夠密度
            float gap = retryGap + Random.Range(0f, 1.5f);
            currentX += gap;
        }
    }

    void SpawnObjectAt(Vector3 pos)
    {
        GameObject prefabToSpawn;
        bool spawnFuel = false;

        // 1. 檢查保底：如果連續敵人次數 >= 設定上限
        if (consecutiveEnemyCount >= maxConsecutiveEnemies)
        {
            spawnFuel = true; // 強制生油
            // Debug.Log("運氣太差，觸發保底機制給予瓦斯！");
        }
        // 2. 如果沒觸發保底，就跑正常的隨機機率
        else
        {
            spawnFuel = (Random.Range(0, 100) < fuelChance);
        }

        // 3. 執行生成並更新計數器
        if (spawnFuel)
        {
            prefabToSpawn = fuelPrefab;
            consecutiveEnemyCount = 0; // 只要出了油，計數器就重置歸零
        }
        else
        {
            prefabToSpawn = enemyPrefab;
            consecutiveEnemyCount++; // 出了敵人，計數器 +1
        }

        Instantiate(prefabToSpawn, pos, Quaternion.identity);
    }

    public void SpawnEnemies(Vector3 bgPos, float bgWidth)
    {
        // 修正：只有在 Playing 狀態才生成敵人
        if (GameManager.Instance.currentState != GameState.Playing) return;

        float startX = bgPos.x - (bgWidth / 2);
        float currentSpawnX = startX;

        while (currentSpawnX < startX + bgWidth)
        {
            float safeGap = playerWidth * 2;
            float randomGap = Random.Range(0, maxRandomGap);
            currentSpawnX += safeGap + randomGap;

            if (currentSpawnX < startX + bgWidth)
            {
                float randomY = Random.Range(minY, maxY);
                Vector3 spawnPos = new Vector3(currentSpawnX, randomY, 0);

                if (Random.Range(0, 100) < fuelChance)
                    Instantiate(fuelPrefab, spawnPos, Quaternion.identity);
                else
                    Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
using UnityEngine;

public class RepeatBackground : MonoBehaviour
{
    //畫面移速
    public float speed = 5f;

    //背景數量
    public int backgroundCount = 2; // 因為你有 BG1 和 BG2，所以設為 2

    private float width; // 圖片的寬度

    private EnemySpawner spawner;// 敵人重生位置

    void Start()
    {
        // 1. 自動抓取圖片寬度
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        width = sr.bounds.size.x;

        spawner = FindObjectOfType<EnemySpawner>();
        
    }

    void Update()
    {
        if (GameManager.Instance.currentState == GameState.GameOver) return;
        // 2. 持續往左移動
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // 3. 檢查是否完全離開畫面左邊
        if (transform.position.x < -width)
        {
            // 4. 進行「接龍」瞬移
            // 為什麼是 width * backgroundCount？
            // 假設寬度是 10，你有 2 張圖。
            // 目前這張在 -10，另一張在 0。
            // 你要把它移到 10 (也就是另一張的後面)。
            // -10 + (10 * 2) = 10。數學成立！

            Vector3 jumpDistance = new Vector3(width * backgroundCount, 0, 0);
            Vector3 newPosition = transform.position + jumpDistance;

            transform.position = newPosition;

            if (spawner != null)
            {
                // 傳入「新位置」和「背景寬度」
                spawner.SpawnEnemies(newPosition, width);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI; // 記得引用 UI 命名空間

public class PlayerHealth : MonoBehaviour
{
    [Header("受傷彈飛力道")]
    public float bounceForce = 500f;

    [Header("數值設定")]
    public int maxHealth = 3;       // 最大血量
    public int currentHealth;       // 當前血量

    [Header("UI 連結")]
    public Slider healthSlider;     // 拖入剛剛做的 Slider

    [Header("音效設定")]
    public AudioClip deflateSound;  // 拖入消氣音效
    private AudioSource audioSource;

    [Header("外觀設定 (新功能)")]
    // 這是一個陣列，可以在 Inspector 裡放多張圖
    // 順序建議：[0]滿血圖, [1]輕傷圖, [2]重傷圖

    [Header("特效設定")]
    public GameObject deathEffect;

    public Sprite[] healthSprites;
    private SpriteRenderer spriteRenderer; // 用來控制顯示的組件

    private BalloonController balloonController; // 用來控制油量和開關
    private bool isDead = false;

    void Start()
    {
        // 1. 初始化血量
        currentHealth = maxHealth;
    
        // 2. 初始化 UI
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // 3. 抓取身上的喇叭組件
        audioSource = GetComponent<AudioSource>();

        // --- 新增：抓取 SpriteRenderer ---
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 遊戲開始時，先更新一次外觀 (確保是滿血圖)
        UpdateSkin();

        balloonController = GetComponent<BalloonController>();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 偵測是否碰到標籤為 "Fuel" 的東西
        if (other.CompareTag("Fuel"))
        {
            BalloonController controller = GetComponent<BalloonController>();

            if (controller != null)
            {
                controller.AddFuel(30f); // 補油
            }

            GameManager.Instance.AddScore(20);

            Destroy(other.gameObject); // 吃掉後銷毀道具
        }
        else if (other.CompareTag("DeathZone"))
        {
            if (isDead) return; // 如果已經死了就不要再觸發

            // 情況 A: 還有血可以扣 (大於 1，因為扣完要是活著)
            if (currentHealth > 1)
            {
                Debug.Log("撞到邊界！扣血並彈回");
                TakeDamage(1);

                if (balloonController != null) balloonController.RestoreFuelPercent(50f);

                // --- 修改這裡：增強反作用力邏輯 ---
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    //  歸零速度
                    rb.velocity = Vector2.zero;

                    //  計算方向：主角位置 - 牆壁中心點 = 往牆壁的反方向
                    Vector2 pushDirection = Vector2.up;

                    // 施加瞬間衝擊力 (Impulse)
                    rb.AddForce(pushDirection * bounceForce, ForceMode2D.Impulse);
                }
            }
            // 情況 B: 沒血可扣了 (剩 1 滴或更少) -> 死亡
            else
            {
                Die();
            }
        }
    }
    // 當發生碰撞時觸發 (記得敵人的 Collider 不能勾 IsTrigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 檢查撞到的是不是障礙物 (需要去設定 Tag)
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            TakeDamage(1);

            // 撞到後銷毀障礙物，避免連續扣血 (或者你可以做無敵時間)
            Destroy(collision.gameObject);
        }
    }

    void TakeDamage(int damage)
    {
        // 扣血
        currentHealth -= damage;

        // 更新 UI
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // --- 新增：更新外觀圖片 ---
        UpdateSkin();

        // 判斷是否還有血
        if (currentHealth > 0)
        {
            // 還有血 -> 播放消氣音效 (PlayOneShot 適合短音效，不會切斷背景音樂)
            if (deflateSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deflateSound);
            }
            Debug.Log("好痛！剩餘血量: " + currentHealth);
        }
        else
        {
            Die();
            // 沒血了 -> 執行死亡邏輯 (例如 Game Over)
            Debug.Log("Game Over!");
            // Time.timeScale = 0; 
        }
    }

    void UpdateSkin()
    {
        // 安全檢查：如果沒有設定圖片或沒有 SpriteRenderer，就跳過，避免報錯
        if (healthSprites.Length == 0 || spriteRenderer == null) return;

        // 計算邏輯：我們要根據血量決定用陣列裡的第幾張圖
        // 假設滿血是 3，圖片陣列有 3 張 [0, 1, 2]

        int spriteIndex = 0;

        if (currentHealth == 4)
        {
            spriteIndex = 0; // 滿血圖
        }
        else if (currentHealth == 3)
        {
            spriteIndex = 1; // 受傷圖
        }
        else if (currentHealth == 2)
        {
            spriteIndex = 2; // 快死掉的圖
        }
        else if (currentHealth == 1)
        {
            spriteIndex = 3; // 死掉的圖
        }
        else
        {
            return; // 0血或負數就不換了(或是換成爆炸圖)
        }

        // 只有當索引值在安全範圍內才換圖
        if (spriteIndex < healthSprites.Length)
        {
            spriteRenderer.sprite = healthSprites[spriteIndex];
        }
    }

    void Die()
    {
        isDead = true;
        currentHealth = 0;
        if (healthSlider != null) healthSlider.value = 0;

        UpdateSkin(); // 更新成死亡圖片

        Debug.Log("失控墜毀！");

        if (deathEffect != null)
        {
            // 在主角身上生成特效
            GameObject gas = Instantiate(deathEffect, transform.position, Quaternion.identity, transform);
        }

        // 1. 切斷氣球控制
        if (balloonController != null)
        {
            balloonController.DisableControl();
        }

        // 2. 通知 GameManager 開始數 3 秒並換場景
        GameManager.Instance.TriggerGameOver();
    }
}
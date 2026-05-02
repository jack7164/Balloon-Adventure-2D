using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// 1. 定義三種遊戲狀態
public enum GameState
{
    Standby,    // 待機 (主選單 / 重試倒數)
    Tutorial,   // 教學中
    Playing,     // 正式遊玩
    GameOver    // 遊戲結束
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 靜態變數
    public static bool isRetry = false;

    [Header("狀態監控")]
    public GameState currentState; // 當前的遊戲狀態

    [Header("UI 設定")]
    public GameObject startMenuUI;   // 開始按鈕介面
    public GameObject hudCanvas;     // 戰鬥介面
    public GameObject pauseMenuUI;   // 暫停介面

    [Header("教學設定")]
    public GameObject[] tutorialImages;
    public float tutorialDuration = 6f;

    [Header("分數時間")]
    public TMP_Text scoreText;
    public TMP_Text timeText;
    private float survivalTime;
    private int score;

    [Header("玩家設定")]
    public Transform playerTransform;
    // 新增：設定遊戲開始時的起跑點 (請在 Inspector 輸入 X:-27.5 Y:-0.4)
    public Vector3 startPosition = new Vector3(-27.5f, -0.4f, 0f);

    private bool isPaused = false;

    private EnemySpawner enemySpawner;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        //抓取場景上的 EnemySpawner
        enemySpawner = FindObjectOfType<EnemySpawner>();

        Time.timeScale = 1f;
        score = 0;
        survivalTime = 0f;
        UpdateScoreUI();
        isPaused = false;

        // 初始化 UI：全部先藏起來
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (hudCanvas != null) hudCanvas.SetActive(false);
        if (startMenuUI != null) startMenuUI.SetActive(false);
        foreach (var img in tutorialImages) if (img != null) img.SetActive(false);

        // --- 核心狀態判斷 ---
        if (isRetry)
        {
            // RETRY 模式：狀態設為 Standby (讓氣球自動飄)，並開始倒數
            currentState = GameState.Standby;
            StartCoroutine(RetrySequence());
        }
        else
        {
            // 第一次進入：狀態設為 Standby，並顯示主選單
            currentState = GameState.Standby;
            if (startMenuUI != null) startMenuUI.SetActive(true);
        }
    }

    // 按下 START 按鈕呼叫此函式
    public void OnStartGameButton()
    {
        if (startMenuUI != null) startMenuUI.SetActive(false);

        // 切換到教學狀態
        currentState = GameState.Tutorial;

        // 開始播放教學
        StartCoroutine(StartGameCountdown());
    }

    // 統一的「開始遊戲」函式 (教學結束或 Retry 倒數結束後呼叫)
    void StartGame()
    {
        // 1. 強制重設主角位置 (歸位)
        if (playerTransform != null)
        {
            playerTransform.position = startPosition;

            Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // 清除殘留速度
                rb.rotation = 0f;
            }
        }

        if (!isRetry && enemySpawner != null)
        {
            Debug.Log("正常開始：提早生成第一波敵人");
            enemySpawner.SpawnEarlyWave();
        }

        // 2. 切換狀態為正式遊玩
        currentState = GameState.Playing;

        // 3. 顯示介面
        if (hudCanvas != null) hudCanvas.SetActive(true);

        Debug.Log("遊戲正式開始！");
    }

    void Update()
    {
        // 只有在 "Playing" 狀態下才執行
        if (currentState == GameState.Playing)
        {
            if (!isPaused)
            {
                survivalTime += Time.deltaTime;
                UpdateTimeUI();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPaused) PauseGame();
            }
            if (isPaused && Input.GetKeyDown(KeyCode.Space))
            {
                ResumeGame();
            }
        }
    }

    // --- 流程協程 ---

    IEnumerator RetrySequence()
    {
        Debug.Log("RETRY 模式：快速啟動中...");
        // 等待 2 秒 (此時狀態是 Standby，氣球會自動飄)
        yield return new WaitForSeconds(2f);
        StartGame();
    }

    IEnumerator StartGameCountdown()
    {
        Debug.Log("教學開始...");
        if (tutorialImages.Length > 0)
        {
            float durationPerSlide = tutorialDuration / tutorialImages.Length;
            for (int i = 0; i < tutorialImages.Length; i++)
            {
                if (tutorialImages[i] != null) tutorialImages[i].SetActive(true);
                yield return new WaitForSeconds(durationPerSlide);
                if (tutorialImages[i] != null) tutorialImages[i].SetActive(false);
            }
        }
        else
        {
            yield return new WaitForSeconds(tutorialDuration);
        }
        StartGame();
    }

    // --- 其他功能 (保持不變) ---
    public void AddScore(int amount) { score += amount; UpdateScoreUI(); }
    void UpdateScoreUI() { if (scoreText != null) scoreText.text = "Score: " + score; }
    void UpdateTimeUI()
    {
        if (timeText != null)
        {
            int m = Mathf.FloorToInt(survivalTime / 60F);
            int s = Mathf.FloorToInt(survivalTime % 60F);
            timeText.text = string.Format("Time: {0:00}:{1:00}", m, s);
        }
    }
    void PauseGame() { isPaused = true; Time.timeScale = 0f; if (pauseMenuUI) pauseMenuUI.SetActive(true); }
    void ResumeGame() { isPaused = false; Time.timeScale = 1f; if (pauseMenuUI) pauseMenuUI.SetActive(false); }
    public void TriggerGameOver() 
    {
        currentState = GameState.GameOver;

        StartCoroutine(GameOverSequence()); 
    }

    IEnumerator GameOverSequence()
    {
        Debug.Log("玩家失去控制...");
        if (playerTransform != null) yield return new WaitUntil(() => playerTransform.position.y < -7f);
        else yield return new WaitForSeconds(3f);

        PlayerPrefs.SetInt("LastScore", score);
        PlayerPrefs.SetFloat("LastTime", survivalTime);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameOverScene");
    }
}
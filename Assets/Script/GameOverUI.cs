using UnityEngine;
using TMPro; // 記得引用 TextMeshPro

public class GameOverUI : MonoBehaviour
{
    [Header("顯示文字 (只顯示數字)")]
    public TMP_Text scoreText; // 對應背景圖的「分數」位置
    public TMP_Text timeText;  // 對應背景圖的「時間」位置
    public TMP_Text gradeText; // 對應背景圖的「等級」位置

    [Header("評分標準設定")]
    public int scoreWeight = 1;  // 1分換算成多少積分
    public int timeWeight = 10;  // 1秒換算成多少積分 (活越久分數越高)

    [Header("等級門檻")]
    public int gradeS = 2000;
    public int gradeA = 1500;
    public int gradeB = 1000;


    void Start()
    {
        // 1. 讀取資料 (如果沒資料預設為 0)
        int finalScore = PlayerPrefs.GetInt("LastScore", 0);
        float finalTime = PlayerPrefs.GetFloat("LastTime", 0f);

        // 2. 顯示數字 (不加標題，因為你的背景圖已經有了)
        if (scoreText != null) scoreText.text = finalScore.ToString();

        if (timeText != null)
        {
            // 將秒數轉為 00:00 格式，或者只顯示純秒數，看你喜好
            // 這裡示範顯示純秒數 (取整數)
            timeText.text = Mathf.FloorToInt(finalTime).ToString();
            // 如果想顯示 00:00 格式，可以用下面這行：
            // int min = Mathf.FloorToInt(finalTime / 60);
            // int sec = Mathf.FloorToInt(finalTime % 60);
            // timeText.text = string.Format("{0:00}:{1:00}", min, sec);
        }

        // 3. 計算總積分 (Grade Calculation)
        // 公式：分數 + (存活秒數 * 權重)
        int totalGradePoints = finalScore * scoreWeight + Mathf.FloorToInt(finalTime * timeWeight);

        Debug.Log("總積分: " + totalGradePoints);

        // 4. 判斷等級並顯示
        if (gradeText != null)
        {
            if (totalGradePoints >= gradeS)
            {
                gradeText.text = "S";
                gradeText.color = Color.yellow; // 金色 S
            }
            else if (totalGradePoints >= gradeA)
            {
                gradeText.text = "A";
                gradeText.color = Color.red;
            }
            else if (totalGradePoints >= gradeB)
            {
                gradeText.text = "B";
                gradeText.color = Color.cyan;
            }
            else
            {
                gradeText.text = "C";
                gradeText.color = Color.gray;
            }
        }
    }
}
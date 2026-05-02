using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // MENU
    public void BackToMenu()
    {
        // 設定靜態變數：告訴 GameManager 下次進去是 "MENU 模式"
        GameManager.isRetry = false;
        SceneManager.LoadScene("GameScene"); // 請改成你的主遊戲場景名稱
    }

    // RETRY
    public void RetryGame()
    {
        // 設定靜態變數：告訴 GameManager 下次進去是 "RETRY 模式"
        GameManager.isRetry = true;
        SceneManager.LoadScene("GameScene");
    }
}
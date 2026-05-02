using UnityEngine;

public class FixTransform : MonoBehaviour
{
    // 設定你截圖中的目標數值
    // Position: X = -400.17, Y = -215.4203, Z = 1
    private Vector3 targetPosition = new Vector3(-400.05f, -215.44f, 1f);

    // Scale: X = 0.4821049, Y = 0.5838288, Z = 0.3803055
    private Vector3 targetScale = new Vector3(0.48f, 0.5908042f, 0.2532352f);

    // 使用 OnEnable，這樣每次教學圖片被 SetActive(true) 打開時，都會自動校正
    void OnEnable()
    {
        // 1. 強制設定位置 (使用 localPosition 以確保相對於父物件的位置正確)
        transform.localPosition = targetPosition;

        // 2. 強制設定大小
        transform.localScale = targetScale;

        // 3. 強制旋轉歸零 (預防萬一)
        transform.localRotation = Quaternion.identity;

        // Debug.Log("已強制修正教學圖片位置與大小");
    }
}
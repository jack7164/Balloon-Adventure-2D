using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    public float speed = 5f;        // 移動速度
    public float deadZone ;   // 超出這個 X 座標就銷毀 (畫面左側外)

    void Update()
    {
        if (GameManager.Instance.currentState == GameState.GameOver) return;
        // 1. 往左飛
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // 2. 飛出畫面後自動銷毀 (節省記憶體)
        deadZone = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x - 1f;
        if (transform.position.x < deadZone)
        {
            Destroy(gameObject);
        }
    }
}
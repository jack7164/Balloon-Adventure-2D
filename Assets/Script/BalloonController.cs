using UnityEngine;
using UnityEngine.UI;

public class BalloonController : MonoBehaviour
{
    [Header("玩家控制開關")]
    public bool canControl = true;

    [Header("物理參數")]
    public float upForce = 15f;
    public float moveSpeed = 5f;
    public float maxUpwardSpeed = 5f;

    [Header("視覺效果")]
    public float inflationScale = 1.1f;
    public float scaleSpeed = 5f;

    [Header("能源系統")]
    public float maxFuel = 100f;
    public float currentFuel;
    public float fuelBurnRate = 10f;
    public Slider fuelSlider;

    private Rigidbody2D rb;
    private float moveInput;
    private Vector3 originalScale;
    private Vector3 targetScale;

    [Header("教學模式設定")]
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 2f;

    private float defaultGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 記錄並暫時關閉重力 (為了待機時的自動懸浮)
        defaultGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        originalScale = transform.localScale;
        targetScale = originalScale;
        currentFuel = maxFuel;
        UpdateFuelUI();
    }

    void Update()
    {
        // 修正：只要不是 Playing 狀態，就不執行玩家控制
        if (GameManager.Instance.currentState != GameState.Playing)
        {
            return;
        }

        // 狀態變為 Playing 後，如果是 0 重力，就恢復重力
        if (rb.gravityScale == 0)
        {
            rb.gravityScale = defaultGravity;
        }

        if (!canControl) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        bool isThrusting = Input.GetKey(KeyCode.Space);
        bool lisThrusting = Input.GetKey(KeyCode.LeftArrow);
        bool risThrusting = Input.GetKey(KeyCode.RightArrow);

        if ((isThrusting && currentFuel > 0)|| (lisThrusting && currentFuel > 0)|| (risThrusting && currentFuel > 0))
        {
            targetScale = originalScale * inflationScale;
            currentFuel -= fuelBurnRate * Time.deltaTime;
        }
        else
        {
            targetScale = originalScale;
        }

        currentFuel = Mathf.Clamp(currentFuel, 0, maxFuel);
        UpdateFuelUI();

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.currentState == GameState.GameOver || !canControl)
        {
            return;
        }

        // 只要不是 Playing 狀態，就執行自動懸浮
        if (GameManager.Instance.currentState != GameState.Playing)
        {
            float hoverVelocity = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            rb.velocity = new Vector2(0, hoverVelocity);
            return;
        }

        if (!canControl) return;

        // --- 物理移動邏輯 ---
        if (Input.GetKey(KeyCode.Space) && currentFuel > 0)
        {
            rb.AddForce(Vector2.up * upForce);
        }

        if (rb.velocity.y > maxUpwardSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxUpwardSpeed);
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    public void RestoreFuelPercent(float percent)
    {
        float amount = maxFuel * (percent / 100f);
        currentFuel += amount;
        if (currentFuel > maxFuel) currentFuel = maxFuel;
        UpdateFuelUI();
    }

    public void DisableControl()
    {
        canControl = false;
        rb.gravityScale = 1f;
        rb.angularVelocity = Random.Range(-50f, 50f);
    }

    public void AddFuel(float amount)
    {
        currentFuel += amount;
        if (currentFuel > maxFuel) currentFuel = maxFuel;
        UpdateFuelUI();
    }

    void UpdateFuelUI()
    {
        if (fuelSlider != null) fuelSlider.value = currentFuel;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text statusText;

    private const float RoundDuration = 60f;
    private float elapsed;

    public bool RoundActive { get; private set; }
    public int Score { get; private set; }

    // 0-20s: 1/s, 20-30s: 2/s, 30-40s: 3/s, 40-50s: 4/s, 50-60s: 5/s
    public float CurrentSpawnRate
    {
        get
        {
            if (elapsed < 20f) return 1f;
            if (elapsed < 30f) return 2f;
            if (elapsed < 40f) return 3f;
            if (elapsed < 50f) return 4f;
            return 5f;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Score = 0;
        elapsed = 0f;
        RoundActive = true;
        if (statusText != null) statusText.text = "";
        UpdateUI();
    }

    private void Update()
    {
        if (!RoundActive) return;

        elapsed += Time.deltaTime;
        if (elapsed >= RoundDuration)
        {
            elapsed = RoundDuration;
            RoundActive = false;
            if (statusText != null) statusText.text = $"遊戲結束！最終分數：{Score}";
        }

        UpdateUI();
    }

    public void AddScore(int amount)
    {
        Score += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"分數 Score: {Score}";
        if (timerText != null) timerText.text = $"時間 Time: {Mathf.CeilToInt(RoundDuration - elapsed)}";
    }
}

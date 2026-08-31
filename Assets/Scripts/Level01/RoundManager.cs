using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Ready,
    Countdown,
    Playing,
    GameOver
}

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text statusText;
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Ready / Countdown 節奏")]
    [SerializeField] private float readyDuration = 1.2f;
    [SerializeField] private float countdownStepDuration = 0.8f;

    // 由 DifficultyData 套用，預設值等同原本的 Normal 基準
    private float roundDuration = 60f;
    private float spawnRateMultiplier = 1f;
    private float scoreMultiplier = 1f;

    private float elapsed;
    private float stateTimer;
    private int countdownStep;

    public GameState State { get; private set; } = GameState.Ready;
    public bool RoundActive => State == GameState.Playing;
    public int Score { get; private set; }
    public float MonsterMoveIntervalMultiplier { get; private set; } = 1f;

    // 0-20s: 1/s, 20-30s: 2/s, 30-40s: 3/s, 40-50s: 4/s, 50-60s: 5/s，再乘上難度倍率
    public float CurrentSpawnRate
    {
        get
        {
            float baseRate;
            if (elapsed < 20f) baseRate = 1f;
            else if (elapsed < 30f) baseRate = 2f;
            else if (elapsed < 40f) baseRate = 3f;
            else if (elapsed < 50f) baseRate = 4f;
            else baseRate = 5f;
            return baseRate * spawnRateMultiplier;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ApplyDifficulty();
        Score = 0;
        elapsed = 0f;
        UpdateUI();
        EnterReady();
    }

    private void ApplyDifficulty()
    {
        DifficultyData diff = GameSessionManager.Instance != null ? GameSessionManager.Instance.SelectedDifficulty : null;
        if (diff == null) return;

        roundDuration = diff.roundDuration;
        spawnRateMultiplier = diff.spawnRateMultiplier;
        scoreMultiplier = diff.scoreMultiplier;
        MonsterMoveIntervalMultiplier = diff.monsterMoveIntervalMultiplier;
    }

    private void Update()
    {
        switch (State)
        {
            case GameState.Ready:
                stateTimer += Time.deltaTime;
                if (stateTimer >= readyDuration)
                {
                    EnterCountdown();
                }
                break;

            case GameState.Countdown:
                stateTimer += Time.deltaTime;
                if (stateTimer >= countdownStepDuration)
                {
                    stateTimer = 0f;
                    countdownStep--;
                    if (countdownStep > 0)
                    {
                        if (statusText != null) statusText.text = countdownStep.ToString();
                    }
                    else if (countdownStep == 0)
                    {
                        if (statusText != null) statusText.text = "GO!";
                    }
                    else
                    {
                        EnterPlaying();
                    }
                }
                break;

            case GameState.Playing:
                elapsed += Time.deltaTime;
                if (elapsed >= roundDuration)
                {
                    elapsed = roundDuration;
                    EnterGameOver();
                }
                UpdateUI();
                break;

            case GameState.GameOver:
                break;
        }
    }

    private void EnterReady()
    {
        State = GameState.Ready;
        stateTimer = 0f;
        if (statusText != null) statusText.text = "READY?";
    }

    private void EnterCountdown()
    {
        State = GameState.Countdown;
        stateTimer = 0f;
        countdownStep = 3;
        if (statusText != null) statusText.text = countdownStep.ToString();
    }

    private void EnterPlaying()
    {
        State = GameState.Playing;
        if (statusText != null) statusText.text = "";
    }

    private void EnterGameOver()
    {
        State = GameState.GameOver;
        if (statusText != null) statusText.text = "";
        if (gameOverUI != null) gameOverUI.Show(Score);
    }

    public void AddScore(int amount)
    {
        Score += Mathf.RoundToInt(amount * scoreMultiplier);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"分數 Score: {Score}";
        if (timerText != null) timerText.text = $"時間 Time: {Mathf.CeilToInt(roundDuration - elapsed)}";
    }
}

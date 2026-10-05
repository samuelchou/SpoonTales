using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private Text livesText;
    [SerializeField] private Text statusText;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private PauseMenuUI pauseMenuUI;

    [Header("Ready / Countdown 節奏")]
    [SerializeField] private float readyDuration = 1.2f;
    [SerializeField] private float countdownStepDuration = 0.8f;

    // 由 DifficultyData 套用，預設值等同原本的 Normal 基準
    private float roundDuration = 60f;
    private float spawnRateMultiplier = 1f;
    private float scoreMultiplier = 1f;
    private int maxLives = 3;

    private float elapsed;
    private float stateTimer;
    private int countdownStep;

    public GameState State { get; private set; } = GameState.Ready;
    public bool IsPaused { get; private set; }
    public bool RoundActive => State == GameState.Playing && !IsPaused;
    public int Score { get; private set; }
    public int CurrentLives { get; private set; }
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

    private void OnDestroy()
    {
        // 暫停中直接切換 Scene 時，確保時間流速被還原
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (pauseMenuUI == null)
        {
            pauseMenuUI = new GameObject("PauseMenu").AddComponent<PauseMenuUI>();
        }
        ApplyDifficulty();
        Score = 0;
        CurrentLives = maxLives;
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
        maxLives = diff.maxLives;
    }

    private void Update()
    {
        if (State != GameState.GameOver && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused) Resume();
            else Pause();
        }

        if (IsPaused) return;

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
                    EnterGameOver(challengeFailed: false);
                }
                UpdateUI();
                break;

            case GameState.GameOver:
                break;
        }
    }

    // 以 Time.timeScale = 0 凍結整個遊戲（怪物、球、計時、生成器都吃 deltaTime）。
    public void Pause()
    {
        if (IsPaused || State == GameState.GameOver) return;
        IsPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null) pauseMenuUI.Show();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null) pauseMenuUI.Hide();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.Load("MainMenu");
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

    private void EnterGameOver(bool challengeFailed)
    {
        State = GameState.GameOver;
        if (statusText != null) statusText.text = "";
        if (gameOverUI != null) gameOverUI.Show(Score, challengeFailed);
    }

    public void AddScore(int amount)
    {
        Score += Mathf.RoundToInt(amount * scoreMultiplier);
        UpdateUI();
    }

    // 怪物漏接（走到 kill zone 還沒被消滅）呼叫這個扣一條命；歸零就提前結束、視為挑戰失敗。
    public void LoseLife()
    {
        if (State != GameState.Playing) return;

        CurrentLives = Mathf.Max(0, CurrentLives - 1);
        UpdateUI();

        if (CurrentLives <= 0)
        {
            EnterGameOver(challengeFailed: true);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"分數 Score: {Score}";
        if (timerText != null) timerText.text = $"時間 Time: {Mathf.CeilToInt(roundDuration - elapsed)}";
        if (livesText != null) livesText.text = $"生命 Lives: {CurrentLives}";
    }
}

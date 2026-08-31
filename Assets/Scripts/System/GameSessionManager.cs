using UnityEngine;

// 跨 Scene 保留「目前選擇的關卡 / 難度」。掛在 MainMenu 裡，DontDestroyOnLoad。
// 如果直接開啟關卡 Scene 測試（沒有經過 MainMenu），Instance 會是 null，
// 使用到的地方要自行 fallback 成預設值，方便單獨開發/測試關卡。
public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [SerializeField] private LevelData defaultLevel;
    [SerializeField] private DifficultyData defaultDifficulty;

    public LevelData SelectedLevel { get; private set; }
    public DifficultyData SelectedDifficulty { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (SelectedLevel == null) SelectedLevel = defaultLevel;
        if (SelectedDifficulty == null) SelectedDifficulty = defaultDifficulty;
    }

    public void SetLevel(LevelData level)
    {
        if (level != null) SelectedLevel = level;
    }

    public void SetDifficulty(DifficultyData difficulty)
    {
        if (difficulty != null) SelectedDifficulty = difficulty;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text finalScoreText;
    [Tooltip("顯示最高分；留空時會複製 finalScoreText 自動放在它下方")]
    [SerializeField] private Text bestScoreText;

    [Header("展場：閒置自動回主選單")]
    [Tooltip("結算畫面停留超過這個秒數沒人操作，就自動回主選單；0 = 不自動返回")]
    [SerializeField] private float autoReturnSeconds = 30f;
    [Tooltip("自動返回前最後幾秒顯示倒數提示")]
    [SerializeField] private float countdownHintSeconds = 10f;

    private const string GameOverTitle = "GAME OVER";
    private const string ChallengeFailedTitle = "挑戰失敗 CHALLENGE FAILED";

    private bool shown;
    private float idleTimer;
    private string bestScoreLine;

    // 注意：不要在這裡把 panel SetActive(false) —— 這個元件就掛在 panel 本身上，
    // 而 panel 預設是關閉的，Unity 會延遲到「panel 第一次被啟用」才觸發 Awake()，
    // 屆時 Show() 剛呼叫完 SetActive(true)，Awake() 又會立刻把它關掉。
    // Panel 的初始隱藏狀態直接由場景檔（Prefab/Scene 存檔時的 active 狀態）負責即可。

    // challengeFailed = true：生命歸零提前結束；false：時間到的正常結束。
    public void Show(int finalScore, bool challengeFailed, int bestScore, bool isNewBest)
    {
        if (titleText != null) titleText.text = challengeFailed ? ChallengeFailedTitle : GameOverTitle;
        if (finalScoreText != null) finalScoreText.text = finalScore.ToString();

        EnsureBestScoreText();
        bestScoreLine = isNewBest ? $"新紀錄！NEW BEST: {bestScore}" : $"最高分 Best: {bestScore}";
        if (bestScoreText != null) bestScoreText.text = bestScoreLine;

        if (panel != null) panel.SetActive(true);
        shown = true;
        idleTimer = 0f;
    }

    private void Update()
    {
        if (!shown || autoReturnSeconds <= 0f) return;

        if (AnyInputThisFrame())
        {
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.unscaledDeltaTime;
        }

        float remaining = autoReturnSeconds - idleTimer;
        if (bestScoreText != null)
        {
            // 倒數提示接在同一行，避免多一行把文字框撐高、壓到下方的按鈕
            bestScoreText.text = remaining <= countdownHintSeconds
                ? $"{bestScoreLine}  <size={Mathf.RoundToInt(bestScoreText.fontSize * 0.7f)}>（{Mathf.CeilToInt(Mathf.Max(0f, remaining))} 秒後返回主選單）</size>"
                : bestScoreLine;
        }

        if (remaining <= 0f)
        {
            shown = false;
            OnMainMenu();
        }
    }

    private static bool AnyInputThisFrame()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.anyKey.wasPressedThisFrame) return true;
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.delta.ReadValue().sqrMagnitude > 0.01f)) return true;
        return false;
    }

    // 場景沒有手動接 bestScoreText 時，複製 finalScoreText 放在它正下方，避免一定要改場景才能用。
    private void EnsureBestScoreText()
    {
        if (bestScoreText != null || finalScoreText == null) return;

        Text clone = Instantiate(finalScoreText, finalScoreText.transform.parent);
        clone.name = "BestScoreValue";
        clone.fontSize = Mathf.Max(14, Mathf.RoundToInt(finalScoreText.fontSize * 0.45f));
        clone.supportRichText = true;
        clone.horizontalOverflow = HorizontalWrapMode.Overflow;
        clone.verticalOverflow = VerticalWrapMode.Overflow;
        // 文字框高度只留一行，緊貼在分數下方
        RectTransform src = finalScoreText.rectTransform;
        RectTransform rt = clone.rectTransform;
        float height = clone.fontSize * 1.4f;
        rt.sizeDelta = new Vector2(Mathf.Max(src.rect.width, 800f), height);
        rt.anchoredPosition = src.anchoredPosition - new Vector2(0f, src.rect.height * 0.5f + height * 0.5f);
        bestScoreText = clone;
    }

    public void OnRetry()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneLoader.Load(current.name);
    }

    public void OnMainMenu()
    {
        SceneLoader.Load("MainMenu");
    }
}

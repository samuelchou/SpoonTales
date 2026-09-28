using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Text titleText;
    [SerializeField] private Text finalScoreText;

    private const string GameOverTitle = "GAME OVER";
    private const string ChallengeFailedTitle = "挑戰失敗 CHALLENGE FAILED";

    // 注意：不要在這裡把 panel SetActive(false) —— 這個元件就掛在 panel 本身上，
    // 而 panel 預設是關閉的，Unity 會延遲到「panel 第一次被啟用」才觸發 Awake()，
    // 屆時 Show() 剛呼叫完 SetActive(true)，Awake() 又會立刻把它關掉。
    // Panel 的初始隱藏狀態直接由場景檔（Prefab/Scene 存檔時的 active 狀態）負責即可。

    // challengeFailed = true：生命歸零提前結束；false：時間到的正常結束。
    public void Show(int finalScore, bool challengeFailed)
    {
        if (titleText != null) titleText.text = challengeFailed ? ChallengeFailedTitle : GameOverTitle;
        if (finalScoreText != null) finalScoreText.text = finalScore.ToString();
        if (panel != null) panel.SetActive(true);
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

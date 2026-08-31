using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Text finalScoreText;

    // 注意：不要在這裡把 panel SetActive(false) —— 這個元件就掛在 panel 本身上，
    // 而 panel 預設是關閉的，Unity 會延遲到「panel 第一次被啟用」才觸發 Awake()，
    // 屆時 Show() 剛呼叫完 SetActive(true)，Awake() 又會立刻把它關掉。
    // Panel 的初始隱藏狀態直接由場景檔（Prefab/Scene 存檔時的 active 狀態）負責即可。

    public void Show(int finalScore)
    {
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

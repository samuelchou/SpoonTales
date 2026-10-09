using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject difficultySelectPanel;

    [SerializeField] private Text selectedLevelText;
    [SerializeField] private Text selectedDifficultyText;

    private void OnEnable()
    {
        ShowMainMenu();
    }

    private void RefreshSelectionDisplay()
    {
        LevelData level = GameSessionManager.Instance != null ? GameSessionManager.Instance.SelectedLevel : null;
        DifficultyData diff = GameSessionManager.Instance != null ? GameSessionManager.Instance.SelectedDifficulty : null;

        if (selectedLevelText != null)
        {
            selectedLevelText.text = TagText("關卡", level != null ? level.levelName : "-");
        }
        if (selectedDifficultyText != null)
        {
            selectedDifficultyText.text = TagText("難度", diff != null ? diff.difficultyId : "-");
        }
    }

    // 吊牌文字：上方小標題（金色 16px），下方數值（20px，顏色用 Text 本身的顏色）
    private static string TagText(string label, string value)
    {
        return "<size=16><color=#E3B548>" + label + "</color></size>\n<size=20>" + value.ToUpperInvariant() + "</size>";
    }

    public void ShowMainMenu()
    {
        RefreshSelectionDisplay();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (difficultySelectPanel != null) difficultySelectPanel.SetActive(false);
    }

    public void ShowLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }

    public void ShowDifficultySelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (difficultySelectPanel != null) difficultySelectPanel.SetActive(true);
    }

    public void OnStartChallenge()
    {
        LevelData level = GameSessionManager.Instance != null ? GameSessionManager.Instance.SelectedLevel : null;
        if (level == null || string.IsNullOrEmpty(level.sceneName)) return;
        SceneLoader.Load(level.sceneName);
    }
}

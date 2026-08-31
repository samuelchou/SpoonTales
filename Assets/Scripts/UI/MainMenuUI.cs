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
            selectedLevelText.text = "關卡\n" + (level != null ? level.levelName : "-");
        }
        if (selectedDifficultyText != null)
        {
            selectedDifficultyText.text = "難度\n" + (diff != null ? diff.difficultyId.ToUpperInvariant() : "-");
        }
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

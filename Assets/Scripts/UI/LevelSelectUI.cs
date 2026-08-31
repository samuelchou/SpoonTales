using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private MainMenuUI mainMenuUI;

    public void SelectLevel(LevelData level)
    {
        if (level == null || !level.isUnlocked) return;

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.SetLevel(level);
        }

        if (mainMenuUI != null) mainMenuUI.ShowMainMenu();
    }

    public void Back()
    {
        if (mainMenuUI != null) mainMenuUI.ShowMainMenu();
    }
}

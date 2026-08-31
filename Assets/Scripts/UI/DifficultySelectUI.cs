using UnityEngine;

public class DifficultySelectUI : MonoBehaviour
{
    [SerializeField] private MainMenuUI mainMenuUI;

    public void SelectDifficulty(DifficultyData difficulty)
    {
        if (difficulty == null) return;

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.SetDifficulty(difficulty);
        }

        if (mainMenuUI != null) mainMenuUI.ShowMainMenu();
    }

    public void Back()
    {
        if (mainMenuUI != null) mainMenuUI.ShowMainMenu();
    }
}

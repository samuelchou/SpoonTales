using UnityEngine;

// 每個「關卡 x 難度」各自保存一個最高分（PlayerPrefs，重開遊戲仍保留）。
// 展場機台用；要清空某個紀錄可呼叫 Reset(key)，或直接刪除 PlayerPrefs。
public static class HighScoreStore
{
    private const string Prefix = "highscore.";

    public static string MakeKey(string levelId, string difficultyId)
    {
        return Prefix + (string.IsNullOrEmpty(levelId) ? "unknown" : levelId) + "." +
               (string.IsNullOrEmpty(difficultyId) ? "default" : difficultyId);
    }

    public static int Get(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }

    // 回傳 true 代表刷新紀錄（分數 > 0 且嚴格大於舊紀錄）
    public static bool Submit(string key, int score, out int best)
    {
        best = Get(key);
        if (score <= 0 || score <= best) return false;

        best = score;
        PlayerPrefs.SetInt(key, score);
        PlayerPrefs.Save();
        return true;
    }

    public static void Reset(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
}

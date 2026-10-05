using UnityEngine;
using UnityEngine.UI;

// 暫停選單：以疊圖方式蓋在遊戲畫面上。UI 於執行時由程式建立（自己的 overlay Canvas），
// 不依賴場景檔內的設定，之後要換成手工排版的 Prefab 時，只要保留 OnResume / OnQuit 兩個入口即可。
public class PauseMenuUI : MonoBehaviour
{
    private GameObject panel;

    private void Awake()
    {
        Build();
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void OnResume()
    {
        if (RoundManager.Instance != null) RoundManager.Instance.Resume();
    }

    public void OnQuit()
    {
        if (RoundManager.Instance != null) RoundManager.Instance.QuitToMainMenu();
        else SceneLoader.Load("MainMenu");
    }

    private void Build()
    {
        var canvasGo = new GameObject("PauseCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // 半透明遮罩同時擋住底下的點擊
        panel = NewRect("PausePanel", canvasGo.transform);
        Stretch(panel.GetComponent<RectTransform>());
        var dim = panel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.75f);

        var title = NewText("Title", panel.transform, "暫停 PAUSED", 64, FontStyle.Bold);
        Place(title.rectTransform, new Vector2(0, 160), new Vector2(800, 100));

        var resume = NewButton("ResumeButton", panel.transform, "繼續遊戲 Resume", OnResume);
        Place((RectTransform)resume.transform, new Vector2(0, 20), new Vector2(480, 90));

        var quit = NewButton("QuitButton", panel.transform, "結束遊戲 Quit", OnQuit);
        Place((RectTransform)quit.transform, new Vector2(0, -100), new Vector2(480, 90));

        var hint = NewText("Hint", panel.transform, "按 Esc 繼續 Press Esc to resume", 28, FontStyle.Normal);
        Place(hint.rectTransform, new Vector2(0, -220), new Vector2(800, 50));
    }

    private static GameObject NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static Text NewText(string name, Transform parent, string content, int size, FontStyle style)
    {
        var go = NewRect(name, parent);
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static Button NewButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var go = NewRect(name, parent);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = NewText("Label", go.transform, label, 36, FontStyle.Normal);
        Stretch(text.rectTransform);
        return button;
    }
}

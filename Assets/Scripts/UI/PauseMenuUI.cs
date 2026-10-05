using UnityEngine;
using UnityEngine.UI;

// 暫停選單：以疊圖方式蓋在遊戲畫面上。UI 於執行時由程式建立（自己的 overlay Canvas），
// 不依賴場景檔內的設定，之後要換成手工排版的 Prefab 時，只要保留 OnResume / OnQuit 兩個入口即可。
public class PauseMenuUI : MonoBehaviour
{
    private GameObject panel;
    private GameObject volumeRows;
    private Slider musicSlider;
    private Slider sfxSlider;

    private void Awake()
    {
        Build();
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);

        // 直接開啟關卡 Scene 測試時沒有 AudioManager，此時不顯示音量調整
        var audio = AudioManager.Instance;
        volumeRows.SetActive(audio != null);
        if (audio != null)
        {
            musicSlider.SetValueWithoutNotify(Mathf.Round(audio.MusicVolume * 100f));
            sfxSlider.SetValueWithoutNotify(Mathf.Round(audio.SoundVolume * 100f));
            RefreshValueLabels();
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.SaveVolume();
    }

    public void OnResume()
    {
        if (RoundManager.Instance != null) RoundManager.Instance.Resume();
    }

    public void OnQuit()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SaveVolume();
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
        Place(title.rectTransform, new Vector2(0, 330), new Vector2(800, 100));

        volumeRows = NewRect("VolumeRows", panel.transform);
        Stretch(volumeRows.GetComponent<RectTransform>());
        musicSlider = BuildVolumeRow("Music", "音樂 Music", 190, v => OnVolumeChanged(true, v), out musicValueText);
        sfxSlider = BuildVolumeRow("Sfx", "音效 SFX", 90, v => OnVolumeChanged(false, v), out sfxValueText);

        var resume = NewButton("ResumeButton", panel.transform, "繼續遊戲 Resume", OnResume);
        Place((RectTransform)resume.transform, new Vector2(0, -50), new Vector2(480, 90));

        var quit = NewButton("QuitButton", panel.transform, "結束遊戲 Quit", OnQuit);
        Place((RectTransform)quit.transform, new Vector2(0, -170), new Vector2(480, 90));

        var hint = NewText("Hint", panel.transform, "按 Esc 繼續 Press Esc to resume", 28, FontStyle.Normal);
        Place(hint.rectTransform, new Vector2(0, -290), new Vector2(800, 50));
    }

    private Text musicValueText;
    private Text sfxValueText;

    private void OnVolumeChanged(bool music, float value)
    {
        var audio = AudioManager.Instance;
        if (audio == null) return;
        if (music) audio.SetMusicVolume(value / 100f);
        else audio.SetSoundVolume(value / 100f);
        RefreshValueLabels();
    }

    private void RefreshValueLabels()
    {
        musicValueText.text = Mathf.RoundToInt(musicSlider.value).ToString();
        sfxValueText.text = Mathf.RoundToInt(sfxSlider.value).ToString();
    }

    // 一列：左邊名稱、中間 Slider（0~100 整數）、右邊目前數值
    private Slider BuildVolumeRow(string name, string label, float y, UnityEngine.Events.UnityAction<float> onChanged, out Text valueText)
    {
        var row = NewRect(name + "Row", volumeRows.transform);
        Place((RectTransform)row.transform, new Vector2(0, y), new Vector2(1000, 60));

        var labelText = NewText("Label", row.transform, label, 32, FontStyle.Normal);
        labelText.alignment = TextAnchor.MiddleLeft;
        Place(labelText.rectTransform, new Vector2(-400, 0), new Vector2(220, 60));

        var slider = NewSlider(name + "Slider", row.transform);
        Place((RectTransform)slider.transform, new Vector2(20, 0), new Vector2(520, 30));
        slider.onValueChanged.AddListener(onChanged);

        valueText = NewText("Value", row.transform, "100", 32, FontStyle.Normal);
        valueText.alignment = TextAnchor.MiddleRight;
        Place(valueText.rectTransform, new Vector2(400, 0), new Vector2(120, 60));
        return slider;
    }

    private static Slider NewSlider(string name, Transform parent)
    {
        var go = NewRect(name, parent);
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        var fillArea = NewRect("Fill Area", go.transform);
        var fillAreaRt = fillArea.GetComponent<RectTransform>();
        Stretch(fillAreaRt);
        fillAreaRt.offsetMin = new Vector2(0, 0);
        fillAreaRt.offsetMax = new Vector2(0, 0);
        var fill = NewRect("Fill", fillArea.transform);
        fill.AddComponent<Image>().color = new Color(0.95f, 0.7f, 0.2f, 1f);
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        var handleArea = NewRect("Handle Slide Area", go.transform);
        var handleAreaRt = handleArea.GetComponent<RectTransform>();
        Stretch(handleAreaRt);
        handleAreaRt.offsetMin = new Vector2(15, 0);
        handleAreaRt.offsetMax = new Vector2(-15, 0);
        var handle = NewRect("Handle", handleArea.transform);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        var handleRt = handle.GetComponent<RectTransform>();
        handleRt.anchorMin = new Vector2(0f, 0f);
        handleRt.anchorMax = new Vector2(0f, 1f);
        handleRt.sizeDelta = new Vector2(30, 24);

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.wholeNumbers = true;
        slider.value = 100;
        return slider;
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

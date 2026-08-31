# 系統 UI 實作筆記

對應 `Spoon_Tales_系統_UI_規格書_v0.1.md` 的實作記錄，說明實際做法、跟規格書的差異，以及過程中踩到的兩個 Unity 陷阱。

## 對應規格書的實作

| 規格書項目 | 實作位置 |
| --- | --- |
| MainMenu Scene | `Assets/Scenes/MainMenu.unity`（Canvas 內含 MainMenuPanel / LevelSelectPanel / DifficultySelectPanel，同時間只顯示一個） |
| LevelData | `Assets/Scripts/System/LevelData.cs`（ScriptableObject），資產放在 `Assets/Data/Levels/` |
| DifficultyData | `Assets/Scripts/System/DifficultyData.cs`（ScriptableObject），資產放在 `Assets/Data/Difficulties/`（Easy / Normal / Hard） |
| GameSessionManager | `Assets/Scripts/System/GameSessionManager.cs`，`DontDestroyOnLoad`，掛在 MainMenu 場景裡 |
| SceneLoader | `Assets/Scripts/System/SceneLoader.cs`，薄包裝 `SceneManager.LoadScene` |
| MainMenuUI / LevelSelectUI / DifficultySelectUI | `Assets/Scripts/UI/` |
| Ready → Countdown → Playing → GameOver 狀態機 | 併入 `RoundManager.cs`（沒有另外拆 `GameplayHUD.cs`，因為狀態機跟計分/計時本來就緊密相關，拆開反而要多傳一堆參照） |
| GameOverUI | `Assets/Scripts/UI/GameOverUI.cs`，Panel 掛在 Level01 Canvas 底下 |

## 難度如何套用（對應規格書「難度是設定，不是 Scene」）

`DifficultyData` 有 4 個欄位，`RoundManager.Start()` 讀取 `GameSessionManager.Instance.SelectedDifficulty` 套用：

- `roundDuration`：直接取代 60 秒基準。
- `spawnRateMultiplier`：乘在原本的生成頻率曲線（1→5 隻/秒）上。
- `monsterMoveIntervalMultiplier`：乘在 `Monster` 每次移動所需時間上，由 `MonsterSpawner` 在生成時指定給每隻怪物。
- `scoreMultiplier`：乘在每次得分上。

數值目前是：Easy（0.7 / 1.3 / 0.8）、Normal（1.0 / 1.0 / 1.0，等同原本基準）、Hard（1.4 / 0.75 / 1.2）。純粹是先抓一組「感覺合理」的初版數字，之後試玩再調整 `Assets/Data/Difficulties/*.asset` 裡的數值即可，不用改程式。

如果直接開啟 `Level01_MonsterShoot.unity` 測試（沒有經過 MainMenu），`GameSessionManager.Instance` 會是 `null`，`RoundManager` 會自動 fallback 成原本的基準值，關卡本身仍然可以獨立開發測試。

## 踩到的兩個 Unity 陷阱

### 1. 用程式碼 `AddListener()` 掛的按鈕事件不會存進場景檔

一開始用 `button.onClick.AddListener(() => ...)` 在 Editor Script 裡把 MainMenu 的按鈕都接好，結果進 Play Mode 之後全部按鈕都沒反應。原因：`AddListener()` 加的是「非持久化」監聽器，只存在於當下的記憶體狀態，**不會被序列化進 `.unity` 場景檔**——不管是進 Play Mode 的 domain reload，還是真正 build 出來的版本，這些監聽器都不會存在。

修法：改用 `UnityEditor.Events.UnityEventTools.AddPersistentListener(...)`（一般方法）跟 `AddObjectPersistentListener<T>(...)`（需要固定帶一個參數的方法，例如 `SelectLevel(LevelData level)`），效果等同在 Inspector 手動點「+」設定 persistent listener，會正確存進場景檔。

### 2. 一開始就關閉的 Panel，`Awake()` 裡不能再呼叫 `SetActive(false)`

`GameOverUI` 原本在 `Awake()` 裡寫 `panel.SetActive(false)`，確保預設隱藏。但 `GameOverPanel` 在場景檔裡本來就是關閉狀態，而 Unity 對「一開始就是關閉的 GameObject」會延遲到**第一次被啟用**才觸發其上元件的 `Awake()`。結果變成：`Show()` 呼叫 `SetActive(true)` → 觸發第一次 `Awake()` → `Awake()` 裡的 `SetActive(false)` 立刻把它關掉，導致 GameOver 畫面永遠打不開。

修法：拿掉 `Awake()` 裡的 `SetActive(false)`，隱藏狀態直接交給場景檔本身的初始 active 狀態負責就好，不要在同一個物件的生命週期方法裡「反向操作自己的啟用狀態」。

## 尚未實作 / 之後可以做的

- LevelSelectPanel 目前只有 Level01 一顆按鈕（規格書允許，未來關卡直接照 `LevelData` 的模式加資產 + 加按鈕即可，不用改架構）。
- GameOver 面板目前是純色半透明底 + 文字，之後套 RPG UI 美術包（`Assets/RPG and MMO UI 5/`）可以直接換皮。
- 難度數值都還沒經過真人試玩調整，是「先抓感覺合理」的初版。

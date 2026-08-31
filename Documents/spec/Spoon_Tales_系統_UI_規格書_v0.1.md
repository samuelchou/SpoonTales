# Spoon Tales - 系統 UI 規格書

> 版本：v0.1  
> 狀態：初版實作規格  
> 用途：定義遊戲系統 UI、Scene Flow 與基本選單架構，供 AI Coding 與程式實作使用。

---

# 1. 文件概述

本文件定義《Spoon Tales》初版的系統 UI 架構。

本遊戲為適合展場展示的街機風格遊戲。每次挑戰應具備容易開始、容易理解，以及可以重複挑戰高分的特性。

初版系統必須支援：

1. 主選單。
2. 直接開始街機挑戰。
3. 選擇遊戲關卡。
4. 選擇遊戲難度。
5. 啟動 `Level01_MonsterShoot`。
6. 遊戲結束後重新挑戰目前關卡。
7. 返回主選單。
8. 未來擴充新的遊戲關卡與難度設定。

UI 應維持簡單、直覺，並適合展場環境操作。

---

# 2. 設計原則

系統 UI 必須遵循以下原則：

1. **簡單**
   - 盡可能減少玩家進入遊戲前所需的操作。
2. **街機導向**
   - 玩家應能快速開始挑戰。
3. **適合展場**
   - 重要按鈕與資訊應清楚、容易辨識。
4. **保留擴充性**
   - 未來新增關卡或難度時，不需要重新設計整個選單系統。
5. **盡可能採用資料驅動**
   - 關卡與難度資訊不應全部直接寫死在個別 UI 元件中。

---

# 3. Scene 架構

## 3.1 初版 Scene

```text
Assets/Scenes/
│
├── MainMenu.unity
│
├── Level01_MonsterShoot.unity
│
├── Level02_xxx.unity          (Future)
│
└── Level03_xxx.unity          (Future)
```

初版實作必須包含：

- `MainMenu`
- `Level01_MonsterShoot`

系統架構必須支援未來新增其他關卡。

---

# 4. 主要流程

```text
MainMenu
    │
    ├── Start Challenge
    │       │
    │       └── Load Selected Level
    │
    ├── Select Level
    │
    └── Select Difficulty

                ↓

       Level01_MonsterShoot

                ↓

              Ready

                ↓

           Countdown

                ↓

             Gameplay

                ↓

            Game Over

          ┌─────┴─────┐
          │           │
          ↓           ↓
        Retry      MainMenu
```

---

# 5. MainMenu Scene

## 5.1 Scene 名稱

```text
MainMenu
```

## 5.2 用途

`MainMenu` 是遊戲的主要入口。

玩家可以：

- 開始目前選擇的街機挑戰。
- 選擇其他遊戲關卡。
- 選擇遊戲難度。

初版設計應優先讓玩家能夠快速進入遊戲。

---

# 6. MainMenu UI 架構

建議 Hierarchy：

```text
Canvas
│
├── MainMenuPanel
│   │
│   ├── GameTitleText
│   │
│   ├── SelectedLevelDisplay
│   │
│   ├── SelectedDifficultyDisplay
│   │
│   ├── StartChallengeButton
│   │
│   ├── SelectLevelButton
│   │
│   └── SelectDifficultyButton
│
├── LevelSelectPanel
│
└── DifficultySelectPanel
```

主要 Panel 同一時間只應顯示一個。

初始狀態：

```text
MainMenuPanel = Visible
LevelSelectPanel = Hidden
DifficultySelectPanel = Hidden
```

---

# 7. MainMenuPanel

`MainMenuPanel` 應包含以下元件。

## 7.1 遊戲標題

顯示：

```text
SPOON TALES
```

正式的中文或英文遊戲名稱未來可以調整，但不應影響選單邏輯。

---

## 7.2 目前選擇的關卡

顯示目前玩家選擇的關卡。

範例：

```text
關卡

Monster Shoot
```

初始預設：

```text
LevelID:
Level01

LevelName:
Monster Shoot
```

---

## 7.3 目前選擇的難度

顯示目前玩家選擇的難度。

範例：

```text
難度

NORMAL
```

初始預設：

```text
Normal
```

---

## 7.4 StartChallengeButton

建議顯示文字：

```text
開始挑戰
```

### 功能

按下後：

1. 讀取目前選擇的關卡。
2. 讀取目前選擇的難度。
3. 儲存目前的遊戲 Session 資料。
4. 載入對應的關卡 Scene。

範例：

```text
SelectedLevel:
Level01_MonsterShoot

SelectedDifficulty:
Normal
```

接著：

```text
LoadScene("Level01_MonsterShoot")
```

---

## 7.5 SelectLevelButton

建議顯示文字：

```text
選擇關卡
```

按下後：

```text
MainMenuPanel = Hidden
LevelSelectPanel = Visible
```

---

## 7.6 SelectDifficultyButton

建議顯示文字：

```text
選擇難度
```

按下後：

```text
MainMenuPanel = Hidden
DifficultySelectPanel = Visible
```

---

# 8. LevelSelectPanel

## 8.1 用途

`LevelSelectPanel` 讓玩家選擇可遊玩的遊戲關卡。

初版目前只有一個可遊玩的關卡：

```text
Level01_MonsterShoot
```

但 UI 與資料架構必須支援未來新增其他關卡。

---

## 8.2 建議 UI Hierarchy

```text
LevelSelectPanel
│
├── TitleText
│
├── LevelListContainer
│   │
│   ├── Level01Button
│   │
│   ├── Level02Button (Future)
│   │
│   └── Level03Button (Future)
│
└── BackButton
```

---

## 8.3 Level01

顯示資訊：

```text
LEVEL 01

Monster Shoot
```

初始狀態：

```text
Playable = True
Unlocked = True
```

選擇後：

```text
SelectedLevel = Level01_MonsterShoot
```

接著：

```text
LevelSelectPanel = Hidden
MainMenuPanel = Visible
```

返回主選單後，主選單必須更新目前選擇的關卡資訊。

---

## 8.4 未來關卡

未來關卡應支援以下資料：

```text
LevelID
LevelName
SceneName
Description
IsUnlocked
```

範例：

```text
LevelID:
Level02

LevelName:
Future Game

SceneName:
Level02_FutureGame

IsUnlocked:
False
```

尚未開放的關卡可以顯示：

```text
COMING SOON
```

或其他鎖定狀態 UI。

---

# 9. DifficultySelectPanel

## 9.1 用途

`DifficultySelectPanel` 讓玩家選擇遊戲難度。

初版提供：

```text
Easy
Normal
Hard
```

預設：

```text
Normal
```

---

## 9.2 建議 UI Hierarchy

```text
DifficultySelectPanel
│
├── TitleText
│
├── EasyButton
│
├── NormalButton
│
├── HardButton
│
└── BackButton
```

---

## 9.3 難度選擇

玩家選擇難度後：

```text
SelectedDifficulty = Selected Difficulty
```

範例：

```text
SelectedDifficulty = Hard
```

接著：

```text
DifficultySelectPanel = Hidden
MainMenuPanel = Visible
```

返回主選單後，必須更新目前選擇的難度。

---

# 10. 難度系統

難度不應對應不同 Scene。

正確的架構應為：

```text
Difficulty
    ↓
DifficultyConfig
    ↓
Selected Level
```

例如：

```text
Easy
    │
Normal
    │
Hard
    │
    └── Level01_MonsterShoot
```

關卡 Scene 讀取目前選擇的難度，並套用對應的遊戲設定。

---

# 11. DifficultyConfig

建議難度設定至少包含：

```text
DifficultyID
RoundDuration
MonsterSpawnRate
MonsterMovementSpeed
ScoreMultiplier
```

未來可以增加：

```text
MonsterTypeProbability
SpawnPattern
MovementPattern
SpecialRules
```

---

## 11.1 Easy

用途：

- 適合第一次遊玩的玩家。
- 較低的怪物生成頻率。
- 較慢的遊戲節奏。

---

## 11.2 Normal

用途：

- 預設街機遊戲體驗。
- 對應目前的初版關卡基準設計。

`Level01_MonsterShoot` 的 Normal 模式，以目前的 60 秒挑戰與逐漸增加的怪物生成頻率作為基準。

---

## 11.3 Hard

用途：

- 提供較高難度的挑戰。
- 更高的怪物生成頻率。
- 更快的遊戲節奏。

---

# 12. GameSessionManager

建議建立一個可跨 Scene 使用的遊戲 Session Manager。

建議責任：

```text
儲存 SelectedLevel
儲存 SelectedDifficulty
提供目前 Session Data
在 Scene 切換之間保留資料
```

建議類別：

```text
GameSessionManager
```

建議保存資料：

```text
SelectedLevel
SelectedDifficulty
```

範例：

```text
SelectedLevel = Level01_MonsterShoot

SelectedDifficulty = Normal
```

此 Manager 必須能夠在 `MainMenu` 與遊戲關卡之間保留。

建議實作方式：

```text
DontDestroyOnLoad
```

如果採用其他方式，也可以，但必須提供相同的跨 Scene 資料保存效果。

---

# 13. LevelData

關卡資訊應使用可重複使用的資料結構表示。

建議欄位：

```text
LevelID
LevelName
SceneName
Description
IsUnlocked
```

範例：

```text
LevelID:
Level01

LevelName:
Monster Shoot

SceneName:
Level01_MonsterShoot

Description:
依照球的顏色與正確時機消滅靠近的怪物。

IsUnlocked:
True
```

---

# 14. Gameplay UI

初版遊戲關卡為：

```text
Level01_MonsterShoot
```

遊戲中的 HUD 應維持簡潔。

---

## 14.1 必要 HUD 元件

```text
Canvas
│
├── ScoreDisplay
│
└── TimeDisplay
```

---

## 14.2 Score Display

位置：

```text
左上角
```

顯示：

```text
SCORE

120
```

每次新的遊戲 Session 開始時，分數必須歸零。

---

## 14.3 Time Display

位置：

```text
右上角
```

顯示：

```text
TIME

42
```

初版關卡使用倒數計時。

基準遊戲時間：

```text
60 秒
```

實際時間可由 DifficultyConfig 決定。

---

# 15. 遊戲開始流程

關卡 Scene 載入後，不應立即開始正式遊戲。

必須有簡單的開始流程。

---

## 15.1 State Flow

```text
Scene Loaded
    ↓
Ready
    ↓
Countdown
    ↓
Gameplay
```

---

## 15.2 Ready State

顯示簡短的準備訊息。

範例：

```text
READY?
```

此狀態下：

```text
Player Input = Disabled
Monster Spawning = Disabled
Game Timer = Not Started
```

---

## 15.3 Countdown

依序顯示：

```text
3
2
1
GO!
```

倒數期間：

```text
Player Input = Disabled
Monster Spawning = Disabled
Game Timer = Not Started
```

---

## 15.4 Gameplay Start

顯示 `GO!` 後：

```text
Player Input = Enabled
Monster Spawning = Enabled
Game Timer = Started
```

接著由 `RoundManager` 開始正常的遊戲流程。

---

# 16. 遊戲狀態

建議遊戲狀態：

```text
Menu
Ready
Countdown
Playing
GameOver
```

未來可選擇增加：

```text
Paused
Result
Loading
```

初版不需要實作暫停系統。

---

# 17. Game Over

當遊戲計時器到達 0 秒：

```text
Playing
    ↓
GameOver
```

系統應：

1. 停止生成新的怪物。
2. 停止主要遊戲計時器。
3. 計算並保存最終分數。
4. 顯示 Game Over UI。

場上既有的球與怪物可以依照原本的邏輯自然結束與清除。

---

# 18. GameOver UI

建議 Hierarchy：

```text
GameOverPanel
│
├── GameOverTitle
│
├── FinalScoreLabel
│
├── FinalScoreValue
│
├── RetryButton
│
└── MainMenuButton
```

---

## 18.1 顯示內容

範例：

```text
GAME OVER


FINAL SCORE

320


[ 再玩一次 ]

[ 返回主選單 ]
```

---

# 19. RetryButton

按下後：

1. 保留目前選擇的關卡。
2. 保留目前選擇的難度。
3. 重新開始目前的關卡。

範例：

```text
Current Level:
Level01_MonsterShoot

Current Difficulty:
Hard
```

系統重新載入：

```text
Level01_MonsterShoot
```

並使用目前的難度設定開始新的挑戰。

---

# 20. MainMenuButton

按下後：

```text
LoadScene("MainMenu")
```

返回 `MainMenu` 後，應繼續顯示目前選擇的：

```text
Level
Difficulty
```

除非未來另行設計重置規則。

---

# 21. 建議 Script Structure

```text
Assets/Scripts/
│
├── System/
│   │
│   ├── GameSessionManager.cs
│   ├── SceneLoader.cs
│   ├── LevelData.cs
│   └── DifficultyData.cs
│
├── UI/
│   │
│   ├── MainMenuUI.cs
│   ├── LevelSelectUI.cs
│   ├── DifficultySelectUI.cs
│   ├── GameplayHUD.cs
│   └── GameOverUI.cs
│
└── Level01/
    │
    ├── RoundManager.cs
    ├── MonsterSpawner.cs
    ├── Monster.cs
    ├── LaneShooter.cs
    └── Ball.cs
```

實際 Script 結構可以調整，但建議維持職責分離。

---

# 22. UI 元件職責

## MainMenuUI

負責：

```text
顯示目前關卡
顯示目前難度
開始挑戰
開啟關卡選擇
開啟難度選擇
```

---

## LevelSelectUI

負責：

```text
顯示可用關卡
選擇關卡
返回主選單 Panel
```

---

## DifficultySelectUI

負責：

```text
顯示難度選項
選擇難度
返回主選單 Panel
```

---

## GameplayHUD

負責：

```text
顯示分數
顯示剩餘時間
顯示遊戲開始倒數
```

---

## GameOverUI

負責：

```text
顯示最終分數
重新挑戰目前關卡
返回 MainMenu
```

---

# 23. 初版 UI Interaction Rules

## 23.1 Main Menu

初始狀態：

```text
SelectedLevel = Level01_MonsterShoot

SelectedDifficulty = Normal
```

---

## 23.2 Start Challenge

```text
Click Start Challenge
    ↓
Save Current Session
    ↓
Load Selected Level
```

---

## 23.3 Select Level

```text
Click Select Level
    ↓
Open LevelSelectPanel
    ↓
Select Level
    ↓
Update SelectedLevel
    ↓
Return MainMenuPanel
```

---

## 23.4 Select Difficulty

```text
Click Select Difficulty
    ↓
Open DifficultySelectPanel
    ↓
Select Difficulty
    ↓
Update SelectedDifficulty
    ↓
Return MainMenuPanel
```

---

## 23.5 Retry

```text
Game Over
    ↓
Click Retry
    ↓
Reload Current Level
    ↓
Keep Current Difficulty
```

---

# 24. 初版實作範圍

以下功能為初版必須實作。

## MainMenu

- [ ] MainMenu Scene。
- [ ] 遊戲標題。
- [ ] 目前關卡顯示。
- [ ] 目前難度顯示。
- [ ] 開始挑戰按鈕。
- [ ] 選擇關卡按鈕。
- [ ] 選擇難度按鈕。

---

## 關卡選擇

- [ ] Level01 可以選擇。
- [ ] Level01 與 Scene 對應。
- [ ] 未來關卡的資料結構。
- [ ] 返回按鈕。

---

## 難度選擇

- [ ] Easy。
- [ ] Normal。
- [ ] Hard。
- [ ] 預設難度：Normal。
- [ ] 難度選擇結果保存。

---

## 遊戲 Session

- [ ] 保存選擇的關卡。
- [ ] 保存選擇的難度。
- [ ] Scene 切換時保留資料。

---

## Gameplay

- [ ] Ready State。
- [ ] Countdown。
- [ ] Gameplay HUD。
- [ ] Score Display。
- [ ] Time Display。
- [ ] Game Over State。

---

## Game Over

- [ ] Final Score Display。
- [ ] Retry Button。
- [ ] Main Menu Button。

---

# 25. 初版不包含的功能

以下功能不屬於初版 UI 實作範圍：

- High Score 排行榜。
- 玩家帳號。
- 多人遊戲。
- 合作模式。
- 成就系統。
- 設定選單。
- 音量設定。
- 語言切換。
- 完整的關卡解鎖系統。
- 玩家存檔資料。
- 暫停選單。

未來可以視需要加入。

---

# 26. 實作注意事項

## 26.1 MainMenu 使用單一 Scene

初版建議：

```text
MainMenu Scene
│
├── MainMenuPanel
├── LevelSelectPanel
└── DifficultySelectPanel
```

初版不需要為「選擇關卡」或「選擇難度」建立額外的 Unity Scene。

這樣可以減少不必要的 Scene Loading，並讓選單系統保持簡單。

---

## 26.2 遊戲關卡使用獨立 Scene

每一個可遊玩的遊戲關卡應為獨立 Scene。

例如：

```text
Level01_MonsterShoot
Level02_xxx
Level03_xxx
```

這樣未來開發不同遊戲內容時，可以保持相對獨立。

---

## 26.3 難度是設定，不是 Scene

不要建立：

```text
Level01_Easy
Level01_Normal
Level01_Hard
```

建議使用：

```text
Level01_MonsterShoot
    +
DifficultyConfig
```

由目前選擇的難度修改遊戲參數。

---

# 27. 最終初版流程

```text
┌─────────────────────────────┐
│          MAIN MENU          │
│                             │
│        SPOON TALES          │
│                             │
│       目前關卡              │
│       Monster Shoot         │
│                             │
│       目前難度              │
│       NORMAL                │
│                             │
│      [ 開始挑戰 ]           │
│                             │
│      [ 選擇關卡 ]           │
│                             │
│      [ 選擇難度 ]           │
│                             │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│  LEVEL01_MONSTERSHOOT       │
│                             │
│          READY?             │
│                             │
│          3 2 1              │
│                             │
│           GO!               │
│                             │
│ SCORE                TIME   │
│ 120                   42    │
│                             │
│        GAMEPLAY             │
│                             │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────┐
│                             │
│         GAME OVER           │
│                             │
│        FINAL SCORE          │
│                             │
│            320              │
│                             │
│       [ 再玩一次 ]          │
│                             │
│      [ 返回主選單 ]         │
│                             │
└─────────────────────────────┘
```

---

# 28. 實作摘要

初版系統必須完成以下核心流程：

```text
MainMenu
    ↓
選擇關卡
選擇難度
    ↓
開始挑戰
    ↓
載入 Level
    ↓
Ready
    ↓
Countdown
    ↓
Gameplay
    ↓
Game Over
    ↓
Retry 或 MainMenu
```

最重要的架構需求如下：

1. `MainMenu` 為獨立的 Unity Scene。
2. 關卡選擇與難度選擇為 `MainMenu` Scene 內的 UI Panel。
3. 每個可遊玩的關卡為獨立 Unity Scene。
4. 難度透過 Gameplay Configuration 傳入關卡。
5. 選擇的關卡與難度必須在 Scene 切換之間保留。
6. 初版可遊玩的關卡為 `Level01_MonsterShoot`。
7. 未來新增關卡時，不應需要重新設計 MainMenu 的基本架構。

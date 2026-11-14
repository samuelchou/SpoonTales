using NUnit.Framework;
using UnityEngine;

// EditMode 測試：Window > General > Test Runner > EditMode > Run All
public class GameLogicTests
{
    [Test]
    public void PickAdjacentLane_AtLeftEdge_MovesRight()
    {
        for (int i = 0; i < 50; i++)
        {
            Assert.AreEqual(1, Monster.PickAdjacentLane(0, 4));
        }
    }

    [Test]
    public void PickAdjacentLane_AtRightEdge_MovesLeft()
    {
        for (int i = 0; i < 50; i++)
        {
            Assert.AreEqual(2, Monster.PickAdjacentLane(3, 4));
        }
    }

    [Test]
    public void PickAdjacentLane_InMiddle_MovesExactlyOneLane()
    {
        Random.InitState(1234);
        bool sawLeft = false, sawRight = false;
        for (int i = 0; i < 200; i++)
        {
            int next = Monster.PickAdjacentLane(1, 4);
            Assert.That(next == 0 || next == 2, $"lane 1 -> {next}");
            sawLeft |= next == 0;
            sawRight |= next == 2;
        }
        Assert.IsTrue(sawLeft && sawRight, "兩個方向都應該出現");
    }

    [Test]
    public void Lanes_AreFourAndSortedLeftToRight()
    {
        Assert.AreEqual(4, Lanes.X.Length);
        for (int i = 1; i < Lanes.X.Length; i++)
        {
            Assert.Less(Lanes.X[i - 1], Lanes.X[i]);
        }
    }

    [Test]
    public void HighScoreStore_OnlyKeepsStrictlyHigherScores()
    {
        string key = HighScoreStore.MakeKey("__test_level__", "__test_diff__");
        HighScoreStore.Reset(key);
        try
        {
            Assert.IsFalse(HighScoreStore.Submit(key, 0, out int best));
            Assert.AreEqual(0, best);

            Assert.IsTrue(HighScoreStore.Submit(key, 50, out best));
            Assert.AreEqual(50, best);

            Assert.IsFalse(HighScoreStore.Submit(key, 50, out best), "同分不算新紀錄");
            Assert.IsFalse(HighScoreStore.Submit(key, 30, out best));
            Assert.AreEqual(50, best);

            Assert.IsTrue(HighScoreStore.Submit(key, 80, out best));
            Assert.AreEqual(80, HighScoreStore.Get(key));
        }
        finally
        {
            HighScoreStore.Reset(key);
        }
    }

    [Test]
    public void HighScoreStore_KeyIsPerLevelAndDifficulty()
    {
        Assert.AreNotEqual(HighScoreStore.MakeKey("Level01", "Easy"), HighScoreStore.MakeKey("Level01", "Hard"));
        Assert.AreNotEqual(HighScoreStore.MakeKey("Level00", "Easy"), HighScoreStore.MakeKey("Level01", "Easy"));
        Assert.AreEqual(HighScoreStore.MakeKey(null, null), HighScoreStore.MakeKey("", ""));
    }

    [Test]
    public void StageData_PickMonsterPrefab_FollowsWeights()
    {
        StageData stage = ScriptableObject.CreateInstance<StageData>();
        GameObject a = new GameObject("A"), b = new GameObject("B"), skipped = new GameObject("Skipped");
        try
        {
            stage.monsters.Add(new StageData.MonsterEntry { prefab = a, weight = 1f });
            stage.monsters.Add(new StageData.MonsterEntry { prefab = skipped, weight = 0f });
            stage.monsters.Add(new StageData.MonsterEntry { prefab = null, weight = 5f });
            stage.monsters.Add(new StageData.MonsterEntry { prefab = b, weight = 3f });

            Assert.AreSame(a, stage.PickMonsterPrefab(0f));
            Assert.AreSame(a, stage.PickMonsterPrefab(0.24f));
            Assert.AreSame(b, stage.PickMonsterPrefab(0.26f));
            Assert.AreSame(b, stage.PickMonsterPrefab(1f)); // Random.value 可能回傳 1
        }
        finally
        {
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(skipped);
            Object.DestroyImmediate(stage);
        }
    }

    [Test]
    public void StageEvent_SpawnsDueAt_SpreadsEvenlyOverInterval()
    {
        var e = new StageData.StageEvent { startTime = 10f, endTime = 20f };

        Assert.AreEqual(0, e.SpawnsDueAt(9.99f, 5), "開始前不生成");
        Assert.AreEqual(1, e.SpawnsDueAt(10f, 5), "開始那一刻生成第一隻");
        Assert.AreEqual(1, e.SpawnsDueAt(11.99f, 5));
        Assert.AreEqual(2, e.SpawnsDueAt(12f, 5), "每 10/5 = 2 秒一隻");
        Assert.AreEqual(5, e.SpawnsDueAt(18f, 5), "最後一隻在結束前");
        Assert.AreEqual(5, e.SpawnsDueAt(100f, 5), "不會超過總數");
        Assert.AreEqual(0, e.SpawnsDueAt(15f, 0));
    }

    [Test]
    public void StageEvent_SpawnsDueAt_ZeroDurationSpawnsAllAtStart()
    {
        var e = new StageData.StageEvent { startTime = 5f, endTime = 5f };
        Assert.AreEqual(0, e.SpawnsDueAt(4.9f, 3));
        Assert.AreEqual(3, e.SpawnsDueAt(5f, 3));
    }

    [Test]
    public void StageEvent_NewEventDefaultsToSpawnMonsters()
    {
        // 既有 asset 裡沒有新欄位的事件，反序列化時要維持原本的生成行為
        var e = new StageData.StageEvent();
        Assert.AreEqual(StageEventType.SpawnMonsters, e.eventType);
        Assert.AreEqual(WalkDirection.Random, e.walkDirection);
        Assert.IsNull(e.amaPrefab);
        Assert.AreEqual(1, e.amaCount, "AmaWalk 事件預設出場一位");
        Assert.AreEqual(0f, e.amaWalkDuration, "0 = 使用 prefab 預設步行時間");
    }

    [Test]
    public void StageEvent_AmaCount_SpreadsWalksOverInterval()
    {
        var e = new StageData.StageEvent { eventType = StageEventType.AmaWalk, startTime = 25f, endTime = 40f, amaCount = 2 };
        Assert.AreEqual(0, e.SpawnsDueAt(24.9f, e.amaCount));
        Assert.AreEqual(1, e.SpawnsDueAt(25f, e.amaCount));
        Assert.AreEqual(1, e.SpawnsDueAt(32.4f, e.amaCount));
        Assert.AreEqual(2, e.SpawnsDueAt(32.5f, e.amaCount));
        Assert.AreEqual(2, e.SpawnsDueAt(60f, e.amaCount));
    }

    [Test]
    public void StageData_PickMonsterPrefab_EmptyListReturnsNull()
    {
        StageData stage = ScriptableObject.CreateInstance<StageData>();
        Assert.IsNull(stage.PickMonsterPrefab(0.5f));
        Object.DestroyImmediate(stage);
    }
}

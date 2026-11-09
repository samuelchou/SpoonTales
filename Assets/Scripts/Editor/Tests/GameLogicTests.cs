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
}

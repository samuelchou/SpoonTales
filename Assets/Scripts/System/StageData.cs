using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

// 關卡進程中可以安排的事件種類；之後要加新事件（例如 Boss、改變走位）就在這裡加一項，
// 再到 StageManager 的 switch 裡處理。
public enum StageEventType
{
    SpawnMonsters,
    AmaWalk
}

public enum WalkDirection
{
    LeftToRight,
    RightToLeft,
    Random
}

// 一個關卡（Stage）的設定，由場景中的 StageManager 引用：
//   monsters：本關可能出現的怪物（生成時依權重抽）
//   progress：關卡進程，每筆事件在 [startTime, endTime) 這段時間內執行
// 想調整這關的怪物或節奏，只要編輯這個 asset，不用改程式。
[CreateAssetMenu(fileName = "StageData", menuName = "SpoonTales/Stage Data")]
public class StageData : ScriptableObject
{
    [Serializable]
    public class MonsterEntry
    {
        [Tooltip("怪物 prefab，必須掛有 Monster 元件；顏色（Kind）與外觀已烘在 prefab 裡")]
        public GameObject prefab;
        [Tooltip("被選中的相對權重；0 代表暫時不出現")]
        [Min(0f)] public float weight = 1f;
    }

    [Serializable]
    public class StageEvent
    {
        public StageEventType eventType = StageEventType.SpawnMonsters;
        [Tooltip("事件開始時間（秒，從 GO! 開始計算）")]
        [Min(0f)] public float startTime;
        [Tooltip("事件結束時間（秒）；等於開始時間代表在開始那一刻一次做完")]
        [Min(0f)] public float endTime = 10f;

        [ShowIf(nameof(eventType), StageEventType.SpawnMonsters)]
        [Tooltip("這段時間內總共生成幾隻怪物（會再乘上難度的 spawnRateMultiplier 並四捨五入），平均分散在區間內")]
        [Min(0)] public int monsterCount = 10;

        [ShowIf(nameof(eventType), StageEventType.AmaWalk)]
        [Tooltip("阿嬤 prefab（掛有 Ama 元件），從一側的門走到另一側的門")]
        public GameObject amaPrefab;
        [ShowIf(nameof(eventType), StageEventType.AmaWalk)]
        [Tooltip("這段時間內總共出場幾次阿嬤（不受難度影響），出場時間平均分散在區間內，第一位在 startTime 出門")]
        [Min(0)] public int amaCount = 1;
        [ShowIf(nameof(eventType), StageEventType.AmaWalk)]
        [Tooltip("每位阿嬤從門走到對面的門花幾秒；0 = 使用 prefab 的預設步行時間")]
        [Min(0f)] public float amaWalkDuration;
        [ShowIf(nameof(eventType), StageEventType.AmaWalk)]
        [Tooltip("從哪一側的門走到哪一側；Random = 每位各自隨機")]
        public WalkDirection walkDirection = WalkDirection.Random;

        public float Duration => Mathf.Max(0f, endTime - startTime);

        // 到了 time 這個時間點，這個事件總共應該已經生成幾隻（共 count 隻）。
        // 第 i 隻（0 起算）在 startTime + i * Duration / count 生成：開始那一刻就出第一隻，
        // 之後平均分散，最後一隻在 endTime 之前。
        public int SpawnsDueAt(float time, int count)
        {
            if (count <= 0 || time < startTime) return 0;
            if (Duration <= 0f || time >= endTime) return count;
            float interval = Duration / count;
            return Mathf.Min(count, Mathf.FloorToInt((time - startTime) / interval) + 1);
        }
    }

    [Header("怪物")]
    [Tooltip("本關可能出現的怪物；每次生成依權重隨機挑一隻")]
    public List<MonsterEntry> monsters = new List<MonsterEntry>();

    [Tooltip("勾選後怪物入場後只左右移動一次，之後只往前走（簡易關卡用）")]
    public bool sideStepOnce;

    [Header("關卡進程")]
    [Tooltip("依時間安排的事件；區間可以重疊（例如同時有兩段生成）")]
    public List<StageEvent> progress = new List<StageEvent>();

    // 依權重隨機挑一個 prefab；清單為空或權重總和為 0 時回傳 null。
    // roll 介於 [0, 1]，抽出來方便測試。
    public GameObject PickMonsterPrefab(float roll)
    {
        float total = 0f;
        foreach (MonsterEntry entry in monsters)
        {
            if (IsValid(entry)) total += entry.weight;
        }
        if (total <= 0f) return null;

        float target = roll * total;
        GameObject last = null;
        foreach (MonsterEntry entry in monsters)
        {
            if (!IsValid(entry)) continue;
            last = entry.prefab;
            if (target < entry.weight) return entry.prefab;
            target -= entry.weight;
        }
        return last; // roll = 1 或浮點誤差時的保險
    }

    public GameObject PickMonsterPrefab() => PickMonsterPrefab(UnityEngine.Random.value);

    private static bool IsValid(MonsterEntry entry) => entry != null && entry.prefab != null && entry.weight > 0f;

    private void OnValidate()
    {
        foreach (StageEvent e in progress)
        {
            if (e != null && e.endTime < e.startTime) e.endTime = e.startTime;
        }
    }
}

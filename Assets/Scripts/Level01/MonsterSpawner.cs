using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject blackMonsterPrefab;
    [SerializeField] private GameObject blueMonsterPrefab;
    [SerializeField] private GameObject whiteMonsterPrefab;

    [Header("怪物種類")]
    [Tooltip("勾選後只會生成黑色怪物（簡易關卡用）")]
    [SerializeField] private bool blackOnly;
    [Tooltip("勾選後怪物入場後只左右移動一次，之後只往前走（簡易關卡用）")]
    [SerializeField] private bool sideStepOnce;

    [Header("出生點")]
    [Tooltip("左右出生點的 |X|，要在畫面外")]
    [SerializeField] private float sideSpawnX = 23f;
    [Tooltip("左右出生點的 Z；遠方出生的怪物也是跑到這個深度才開始正常走位")]
    [SerializeField] private float sideSpawnZ = 14f;
    [Tooltip("遠方中央出生點的 Z（未來「傳送門」門框的位置）")]
    [SerializeField] private float farSpawnZ = 30f;
    [SerializeField] private float monsterScale = 2f;

    [Header("入場時間（秒，會再乘上難度的速度倍率）")]
    [SerializeField] private float sideEntryDuration = 0.5f;
    [SerializeField] private float farEntryDuration = 0.8f;

    private float timer;

    private void Update()
    {
        if (RoundManager.Instance == null || !RoundManager.Instance.RoundActive)
        {
            return;
        }

        timer += Time.deltaTime;
        float interval = 1f / RoundManager.Instance.CurrentSpawnRate;
        if (timer >= interval)
        {
            timer -= interval;
            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        // 賽道由左到右為 0..3（即 1..4 賽道）。每個出生點固定進入靠近它的賽道：
        //   左邊 → 最左賽道；右邊 → 最右賽道；遠方中央 → 中間兩條賽道（隨機）
        Vector3 spawnPos;
        int entryLane;
        float entryDuration;
        int lastLane = Lanes.X.Length - 1;
        switch (Random.Range(0, 3)) // 0 = left, 1 = right, 2 = far center
        {
            case 0:
                spawnPos = new Vector3(-sideSpawnX, Lanes.Y, sideSpawnZ);
                entryLane = 0;
                entryDuration = sideEntryDuration;
                break;
            case 1:
                spawnPos = new Vector3(sideSpawnX, Lanes.Y, sideSpawnZ);
                entryLane = lastLane;
                entryDuration = sideEntryDuration;
                break;
            default:
                spawnPos = new Vector3(0f, Lanes.Y, farSpawnZ);
                entryLane = Random.Range(1, lastLane); // 中間賽道（4 條賽道時為 1 或 2）
                entryDuration = farEntryDuration;
                break;
        }
        Vector3 entryTarget = new Vector3(Lanes.X[entryLane], Lanes.Y, sideSpawnZ);

        SpoonColor kind = blackOnly ? SpoonColor.Black : (SpoonColor)Random.Range(0, 3);
        GameObject prefab = kind switch
        {
            SpoonColor.Black => blackMonsterPrefab,
            SpoonColor.Blue => blueMonsterPrefab,
            _ => whiteMonsterPrefab
        };

        // 顏色（Kind）與外觀已經烘進各自的 prefab 裡，這裡只需要生成、擺位置、套用目前難度速度倍率。
        GameObject go = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
        go.name = "Monster_" + kind;
        go.transform.localScale = Vector3.one * monsterScale;

        Monster monster = go.GetComponent<Monster>();
        float speedMultiplier = RoundManager.Instance.MonsterMoveIntervalMultiplier;
        monster.MoveIntervalMultiplier = speedMultiplier;
        monster.SideStepOnce = sideStepOnce;
        monster.SetEntry(entryTarget, entryDuration * speedMultiplier);
    }
}

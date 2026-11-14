using UnityEngine;

// 管理單一關卡（Stage）的進行：依 StageData 的關卡進程（progress）在各時間區間執行事件，
// 例如在區間內平均生成指定數量的怪物（從 StageData 的怪物清單抽）。時間用 RoundManager.Elapsed。
public class StageManager : MonoBehaviour
{
    [Tooltip("本關的怪物生成設定（可能出現的怪物清單等）")]
    [SerializeField] private StageData stageData;

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

    [Header("阿嬤（AmaWalk 事件）")]
    [Tooltip("左側門的位置；有指定 Transform 時優先使用 Transform 的位置")]
    [SerializeField] private Transform amaLeftDoor;
    [SerializeField] private Vector3 amaLeftDoorPosition = new Vector3(-7.9f, 3.5f, 3.6f);
    [Tooltip("右側門的位置；有指定 Transform 時優先使用 Transform 的位置")]
    [SerializeField] private Transform amaRightDoor;
    [SerializeField] private Vector3 amaRightDoorPosition = new Vector3(7.9f, 3.5f, 3.6f);

    // 每個進程事件已經生成了幾隻（index 對應 stageData.progress）
    private int[] spawnedPerEvent;

    private void Start()
    {
        int count = stageData != null ? stageData.progress.Count : 0;
        spawnedPerEvent = new int[count];
        if (stageData == null)
        {
            Debug.LogWarning($"{name}: 沒有指定 StageData，本關不會生成怪物", this);
        }
    }

    private void Update()
    {
        if (stageData == null || RoundManager.Instance == null || !RoundManager.Instance.RoundActive)
        {
            return;
        }

        float time = RoundManager.Instance.Elapsed;
        for (int i = 0; i < stageData.progress.Count && i < spawnedPerEvent.Length; i++)
        {
            StageData.StageEvent stageEvent = stageData.progress[i];
            if (stageEvent == null) continue;

            switch (stageEvent.eventType)
            {
                case StageEventType.SpawnMonsters:
                    int total = Mathf.RoundToInt(stageEvent.monsterCount * RoundManager.Instance.SpawnRateMultiplier);
                    int due = stageEvent.SpawnsDueAt(time, total);
                    // 卡頓時一幀可能補生成好幾隻，確保區間內的總數不變
                    while (spawnedPerEvent[i] < due)
                    {
                        spawnedPerEvent[i]++;
                        SpawnOne();
                    }
                    break;

                case StageEventType.AmaWalk:
                    // amaCount 位阿嬤平均分散在區間內出門（與怪物生成同樣的時間分配方式）
                    int amaDue = stageEvent.SpawnsDueAt(time, stageEvent.amaCount);
                    while (spawnedPerEvent[i] < amaDue)
                    {
                        spawnedPerEvent[i]++;
                        SpawnAma(stageEvent);
                    }
                    break;
            }
        }
    }

    private void SpawnAma(StageData.StageEvent stageEvent)
    {
        if (stageEvent.amaPrefab == null)
        {
            Debug.LogWarning($"{name}: AmaWalk 事件（{stageEvent.startTime}s）沒有指定 amaPrefab", this);
            return;
        }

        Vector3 left = amaLeftDoor != null ? amaLeftDoor.position : amaLeftDoorPosition;
        Vector3 right = amaRightDoor != null ? amaRightDoor.position : amaRightDoorPosition;
        bool leftToRight = stageEvent.walkDirection switch
        {
            WalkDirection.LeftToRight => true,
            WalkDirection.RightToLeft => false,
            _ => Random.value < 0.5f
        };
        Vector3 from = leftToRight ? left : right;
        Vector3 to = leftToRight ? right : left;

        GameObject go = Object.Instantiate(stageEvent.amaPrefab, from, Quaternion.identity);
        Ama ama = go.GetComponent<Ama>();
        if (ama == null)
        {
            Debug.LogWarning($"{name}: amaPrefab 上沒有 Ama 元件", go);
            Destroy(go);
            return;
        }
        ama.Walk(from, to, stageEvent.amaWalkDuration);
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

        GameObject prefab = stageData != null ? stageData.PickMonsterPrefab() : null;
        if (prefab == null)
        {
            Debug.LogWarning($"{name}: StageData 沒有可生成的怪物，略過這次生成", this);
            return;
        }

        // 顏色（Kind）與外觀已經烘進各自的 prefab 裡，這裡只需要生成、擺位置、套用目前難度速度倍率。
        GameObject go = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * monsterScale;

        Monster monster = go.GetComponent<Monster>();
        go.name = "Monster_" + monster.Kind;
        float speedMultiplier = RoundManager.Instance.MonsterMoveIntervalMultiplier;
        monster.MoveIntervalMultiplier = speedMultiplier;
        monster.SideStepOnce = stageData.sideStepOnce;
        monster.SetEntry(entryTarget, entryDuration * speedMultiplier);
    }
}

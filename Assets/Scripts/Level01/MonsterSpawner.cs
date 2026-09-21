using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject blackMonsterPrefab;
    [SerializeField] private GameObject blueMonsterPrefab;
    [SerializeField] private GameObject whiteMonsterPrefab;

    [SerializeField] private float sideSpawnX = 20f;
    [SerializeField] private float sideSpawnZ = 14f;
    [SerializeField] private float farSpawnZ = 30f;
    [SerializeField] private float monsterScale = 2f;

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
        Vector3 spawnPos;
        int origin = Random.Range(0, 3); // 0 = left, 1 = right, 2 = far
        switch (origin)
        {
            case 0:
                spawnPos = new Vector3(-sideSpawnX, Lanes.Y, sideSpawnZ);
                break;
            case 1:
                spawnPos = new Vector3(sideSpawnX, Lanes.Y, sideSpawnZ);
                break;
            default:
                spawnPos = new Vector3(Lanes.X[Random.Range(0, Lanes.X.Length)], Lanes.Y, farSpawnZ);
                break;
        }

        SpoonColor kind = (SpoonColor)Random.Range(0, 3);
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
        monster.MoveIntervalMultiplier = RoundManager.Instance.MonsterMoveIntervalMultiplier;
    }
}

using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private Sprite blackSprite;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite whiteSprite;

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
        Sprite sprite = kind switch
        {
            SpoonColor.Black => blackSprite,
            SpoonColor.Blue => blueSprite,
            _ => whiteSprite
        };

        GameObject go = new GameObject("Monster_" + kind);
        go.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * monsterScale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        SphereCollider col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Monster monster = go.AddComponent<Monster>();
        monster.Kind = kind;
        monster.MoveIntervalMultiplier = RoundManager.Instance.MonsterMoveIntervalMultiplier;
    }
}

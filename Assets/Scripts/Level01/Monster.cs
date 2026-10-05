using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Monster : MonoBehaviour
{
    public SpoonColor Kind;

    // 由 MonsterSpawner 依目前難度設定；1 = 基準速度，小於 1 代表移動更快
    public float MoveIntervalMultiplier = 1f;

    [SerializeField] private float moveInterval = 1f / 3f; // 每秒 3 動：每次移動（左右或前進）都花這麼久
    [SerializeField] private float forwardStep = 3f;
    [SerializeField] private float killZoneZ = 1.5f;
    [SerializeField] private int scoreValue = 10;

    private int currentLane;
    private bool hasEntry;
    private Vector3 entryTarget;
    private float entryDuration;

    // 由 MonsterSpawner 在生成後呼叫：怪物先直接移動到 target（入場位置，通常是某條賽道上），
    // 完成後才開始正常的「左右移動 4 次 → 前進 1 次」循環。
    // duration 是入場花費的秒數（已含難度速度倍率，由呼叫端算好）。
    public void SetEntry(Vector3 target, float duration)
    {
        hasEntry = true;
        entryTarget = target;
        entryDuration = duration;
    }

    private void Start()
    {
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator EntryRoutine()
    {
        Vector3 start = transform.position;
        float t = 0f;
        while (t < entryDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, entryTarget, t / entryDuration);
            yield return null;
        }
        transform.position = entryTarget;
    }

    private IEnumerator MoveRoutine()
    {
        if (hasEntry)
        {
            yield return StartCoroutine(EntryRoutine());
        }

        currentLane = NearestLane(transform.position.x);

        while (transform.position.z > killZoneZ)
        {
            for (int i = 0; i < 4 && transform.position.z > killZoneZ; i++)
            {
                yield return StartCoroutine(ShuffleOnce());
            }

            if (transform.position.z <= killZoneZ)
            {
                break;
            }

            yield return StartCoroutine(StepForward());
        }

        // 漏接：怪物一路走到玩家面前都沒被消滅，扣一條命。
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.LoseLife();
        }

        Destroy(gameObject);
    }

    // 左右走位每次只移動「相鄰的一條賽道」（左 1 或右 1），這樣每次位移距離一樣、速度均勻，
    // 不會出現 1 -> 4 這種跨多條賽道的瞬移。在最左／最右賽道時只能往內側移動。
    public static int PickAdjacentLane(int currentLane, int laneCount)
    {
        if (currentLane <= 0) return 1;
        if (currentLane >= laneCount - 1) return laneCount - 2;
        return Random.value < 0.5f ? currentLane - 1 : currentLane + 1;
    }

    private static int NearestLane(float x)
    {
        int nearest = 0;
        for (int i = 1; i < Lanes.X.Length; i++)
        {
            if (Mathf.Abs(Lanes.X[i] - x) < Mathf.Abs(Lanes.X[nearest] - x))
            {
                nearest = i;
            }
        }
        return nearest;
    }

    private IEnumerator ShuffleOnce()
    {
        currentLane = PickAdjacentLane(currentLane, Lanes.X.Length);
        Vector3 start = transform.position;
        Vector3 target = new Vector3(Lanes.X[currentLane], start.y, start.z);

        float duration = moveInterval * MoveIntervalMultiplier;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.position = target;
    }

    private IEnumerator StepForward()
    {
        Vector3 start = transform.position;
        Vector3 target = start + new Vector3(0f, 0f, -forwardStep);

        float duration = moveInterval * MoveIntervalMultiplier;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.position = target;
    }

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball == null || ball.Kind != Kind)
        {
            return;
        }

        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.AddScore(scoreValue);
        }

        AudioManager.Instance?.PlaySfx("hit_monster");
        Destroy(ball.gameObject);
        StopAllCoroutines();
        Destroy(gameObject);
    }
}

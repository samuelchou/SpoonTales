using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Monster : MonoBehaviour
{
    public SpoonColor Kind;

    [SerializeField] private float moveInterval = 1f / 3f; // 每秒 3 動：每次移動（左右或前進）都花這麼久
    [SerializeField] private float forwardStep = 3f;
    [SerializeField] private float killZoneZ = 1.5f;
    [SerializeField] private int scoreValue = 10;

    private void Start()
    {
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
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

        Destroy(gameObject);
    }

    private IEnumerator ShuffleOnce()
    {
        int laneIndex = Random.Range(0, Lanes.X.Length);
        Vector3 start = transform.position;
        Vector3 target = new Vector3(Lanes.X[laneIndex], start.y, start.z);

        float t = 0f;
        while (t < moveInterval)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, t / moveInterval);
            yield return null;
        }
        transform.position = target;
    }

    private IEnumerator StepForward()
    {
        Vector3 start = transform.position;
        Vector3 target = start + new Vector3(0f, 0f, -forwardStep);

        float t = 0f;
        while (t < moveInterval)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, t / moveInterval);
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

        Destroy(ball.gameObject);
        StopAllCoroutines();
        Destroy(gameObject);
    }
}

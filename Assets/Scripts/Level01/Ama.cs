using System.Collections;
using UnityEngine;

// 阿嬤：從一側的門走到另一側的門，走完就消失。不能打她——球打到阿嬤會扣一條命。
// 由 StageManager 依 StageData 的 AmaWalk 事件生成，並呼叫 Walk() 決定起點、終點與步行時間。
[RequireComponent(typeof(Rigidbody))]
public class Ama : MonoBehaviour
{
    [Tooltip("Walk() 傳入的步行時間 <= 0 時使用的預設秒數")]
    [SerializeField] private float defaultWalkDuration = 8f;
    [Tooltip("每被球打到一次扣幾條命")]
    [SerializeField] private int livesLostPerHit = 1;

    [Header("Sfx")]
    [SerializeField] private string hitSfxName = "hit_monster";

    [Header("朝向")]
    [Tooltip("圖片素材本身是否朝右；素材朝左就取消勾選")]
    [SerializeField] private bool artFacesRight = true;
    [Tooltip("要翻轉 / 閃紅的 SpriteRenderer；留空會自動在自己或子物件中尋找")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("被打到的回饋")]
    [SerializeField] private Color hitColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private float hitFlashDuration = 0.3f;

    private Coroutine flashRoutine;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    // 從 from 走到 to，花 duration 秒（<= 0 時用 defaultWalkDuration），到了就銷毀自己。
    public void Walk(Vector3 from, Vector3 to, float duration)
    {
        StopAllCoroutines();
        flashRoutine = null;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        StartCoroutine(WalkRoutine(from, to, duration > 0f ? duration : defaultWalkDuration));
    }

    private IEnumerator WalkRoutine(Vector3 from, Vector3 to, float duration)
    {
        transform.position = from;
        FaceDirection(to.x - from.x);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        transform.position = to;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball == null)
        {
            return;
        }

        // 任何顏色的球打到阿嬤都算誤傷
        Destroy(ball.gameObject);
        AudioManager.Instance?.PlaySfx(hitSfxName);
        if (RoundManager.Instance != null)
        {
            for (int i = 0; i < livesLostPerHit; i++)
            {
                RoundManager.Instance.LoseLife();
            }
        }

        if (spriteRenderer != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = Color.white;
        flashRoutine = null;
    }

    private void FaceDirection(float dx)
    {
        if (Mathf.Abs(dx) < 0.01f || spriteRenderer == null)
        {
            return;
        }
        spriteRenderer.flipX = (dx > 0f) != artFacesRight;
    }
}

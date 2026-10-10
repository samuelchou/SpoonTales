using UnityEngine;

// 主選單裝飾：一群鵝在畫面底部左右走動。位置每幀依攝影機 viewport 計算，
// 所以開場運鏡期間鵝也會固定在畫面底部。被 SpoonCursor 砸中的鵝會播放死亡動畫，之後從畫面邊緣重新走進來。
public class GooseParade : MonoBehaviour
{
    [Tooltip("留空會使用 Camera.main")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RuntimeAnimatorController walkController;
    [SerializeField] private RuntimeAnimatorController dieController;
    [Tooltip("SpriteRenderer 使用的材質；留空用預設材質")]
    [SerializeField] private Material spriteMaterial;

    [SerializeField] private int gooseCount = 8;
    [Tooltip("鵝與攝影機的距離；太遠會插進地板（攝影機往下看），保持在地面之前")]
    [SerializeField] private float depth = 4f;
    [Tooltip("鵝在畫面中的高度範圍（viewport 0~1，0 是畫面最底）")]
    [SerializeField] private Vector2 viewportYRange = new Vector2(0.07f, 0.17f);
    [Tooltip("每秒走過的畫面寬度比例")]
    [SerializeField] private Vector2 speedRange = new Vector2(0.03f, 0.07f);
    [Tooltip("縮放範圍；越靠畫面底部的鵝越大，營造遠近感")]
    [SerializeField] private Vector2 scaleRange = new Vector2(0.18f, 0.28f);
    [Tooltip("圖片素材本身是否朝右；素材朝左就取消勾選")]
    [SerializeField] private bool artFacesRight = true;
    [SerializeField] private string dieSfxName;

    // 鵝走出畫面多遠後才從另一側繞回來（viewport 比例）
    private const float Margin = 0.15f;
    // 死亡動畫播完後，隔多久才重新出場
    private const float RespawnDelay = 1f;

    private class Goose
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public Animator animator;
        public float x;
        public float y;
        public float speed;
        public int dir;
        // >= 0 表示正在死亡，數到 0 以下就重新出場
        public float deadTimer = -1f;
    }

    private Goose[] geese;
    private float dieLength;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (dieController != null)
        {
            foreach (AnimationClip clip in dieController.animationClips)
            {
                dieLength = Mathf.Max(dieLength, clip.length);
            }
        }

        geese = new Goose[gooseCount];
        for (int i = 0; i < gooseCount; i++)
        {
            geese[i] = CreateGoose(i);
        }
    }

    private Goose CreateGoose(int index)
    {
        GameObject go = new GameObject("Goose_" + index);
        go.transform.SetParent(transform, false);

        Goose goose = new Goose
        {
            transform = go.transform,
            x = Random.Range(-Margin, 1f + Margin),
            y = Random.Range(viewportYRange.x, viewportYRange.y),
            speed = Random.Range(speedRange.x, speedRange.y),
            dir = Random.value < 0.5f ? -1 : 1
        };

        float near = Mathf.InverseLerp(viewportYRange.y, viewportYRange.x, goose.y);
        go.transform.localScale = Vector3.one * Mathf.Lerp(scaleRange.x, scaleRange.y, near);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        goose.renderer = sr;
        if (spriteMaterial != null)
        {
            sr.sharedMaterial = spriteMaterial;
        }
        sr.flipX = (goose.dir > 0) != artFacesRight;
        // 越靠畫面底部（越近）的鵝畫在越前面
        sr.sortingOrder = Mathf.RoundToInt(near * 100f);

        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = walkController;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.Play(0, 0, Random.value); // 錯開步伐，避免整群同步
        goose.animator = animator;

        return goose;
    }

    // 砸死畫面上覆蓋 screenPoint 的所有鵝，回傳砸死的數量
    public int KillAt(Vector2 screenPoint)
    {
        if (targetCamera == null || dieController == null)
        {
            return 0;
        }

        int killed = 0;
        foreach (Goose goose in geese)
        {
            if (goose.deadTimer >= 0f || !ContainsScreenPoint(goose, screenPoint))
            {
                continue;
            }
            goose.deadTimer = dieLength + RespawnDelay;
            goose.animator.runtimeAnimatorController = dieController;
            goose.animator.Play(0, 0, 0f);
            killed++;
        }
        AudioManager.Instance.PlaySfx(dieSfxName);
        return killed;
    }

    private bool ContainsScreenPoint(Goose goose, Vector2 screenPoint)
    {
        Sprite sprite = goose.renderer.sprite;
        if (sprite == null)
        {
            return false;
        }

        Bounds bounds = sprite.bounds;
        Vector3 a = targetCamera.WorldToScreenPoint(goose.transform.TransformPoint(bounds.min));
        Vector3 b = targetCamera.WorldToScreenPoint(goose.transform.TransformPoint(bounds.max));
        return screenPoint.x >= Mathf.Min(a.x, b.x) && screenPoint.x <= Mathf.Max(a.x, b.x)
            && screenPoint.y >= Mathf.Min(a.y, b.y) && screenPoint.y <= Mathf.Max(a.y, b.y);
    }

    private void UpdateDead(Goose goose)
    {
        goose.deadTimer -= Time.unscaledDeltaTime;
        if (goose.deadTimer > dieLength)
        {
            return;
        }
        // 死亡動畫播完：先藏起來，等待結束後從行進方向的畫面外重新出場
        goose.renderer.enabled = false;
        if (goose.deadTimer >= 0f)
        {
            return;
        }

        goose.deadTimer = -1f;
        goose.x = goose.dir > 0 ? -Margin : 1f + Margin;
        goose.renderer.enabled = true;
        goose.animator.runtimeAnimatorController = walkController;
        goose.animator.Play(0, 0, Random.value);
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Quaternion facing = targetCamera.transform.rotation;
        foreach (Goose goose in geese)
        {
            if (goose.deadTimer >= 0f)
            {
                UpdateDead(goose);
            }
            else
            {
                goose.x += goose.dir * goose.speed * Time.unscaledDeltaTime;
            }

            if (goose.x > 1f + Margin)
            {
                goose.x = -Margin;
            }
            else if (goose.x < -Margin)
            {
                goose.x = 1f + Margin;
            }

            goose.transform.SetPositionAndRotation(
                targetCamera.ViewportToWorldPoint(new Vector3(goose.x, goose.y, depth)),
                facing);
        }
    }
}

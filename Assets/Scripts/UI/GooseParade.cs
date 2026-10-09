using UnityEngine;

// 主選單裝飾：一群鵝在畫面底部左右走動。位置每幀依攝影機 viewport 計算，
// 所以開場運鏡期間鵝也會固定在畫面底部。
public class GooseParade : MonoBehaviour
{
    [Tooltip("留空會使用 Camera.main")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RuntimeAnimatorController walkController;
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

    // 鵝走出畫面多遠後才從另一側繞回來（viewport 比例）
    private const float Margin = 0.15f;

    private class Goose
    {
        public Transform transform;
        public float x;
        public float y;
        public float speed;
        public int dir;
    }

    private Goose[] geese;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
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

        return goose;
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
            goose.x += goose.dir * goose.speed * Time.unscaledDeltaTime;
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

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 主選單滑鼠游標：一支湯匙跟著指標移動，點擊時播放 spoon-fall 動畫倒下，
// 落地瞬間若砸到 GooseParade 的鵝，鵝就播放死亡動畫。
// spoon-fall 動畫驅動的是 SpriteRenderer.sprite，所以用一個不渲染的 SpriteRenderer 播動畫，
// 每幀把 sprite 複製到最上層 Overlay Canvas 的 Image，確保湯匙畫在所有 UI 之上。
public class SpoonCursor : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController fallController;
    [Tooltip("落地時要檢查是否砸到的鵝群；留空會在場景中尋找")]
    [SerializeField] private GooseParade gooseParade;
    [Tooltip("游標大小（1920x1080 參考解析度像素）")]
    [SerializeField] private Vector2 size = new Vector2(48f, 96f);
    [Tooltip("動畫播放到多少比例算是落地（0~1；spoon-fall 12 格中的第 7 格 = 0.5）。用比例而非秒數，調整動畫速度或長度都不用改")]
    [Range(0f, 1f)]
    [SerializeField] private float impactPoint = 0.5f;
    [SerializeField] private int sortingOrder = 1000;

    private RectTransform spoonRect;
    private Image spoonImage;
    private SpriteRenderer spriteSource;
    private Animator animator;
    private bool isFalling;
    private bool impacted;

    private void Awake()
    {
        if (gooseParade == null)
        {
            gooseParade = FindAnyObjectByType<GooseParade>();
        }

        // 播放動畫用的 SpriteRenderer：保持啟用讓 Animator 能寫入，但不實際渲染
        GameObject source = new GameObject("SpoonAnimator");
        source.transform.SetParent(transform, false);
        spriteSource = source.AddComponent<SpriteRenderer>();
        spriteSource.forceRenderingOff = true;
        animator = source.AddComponent<Animator>();
        animator.runtimeAnimatorController = fallController;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        ResetToIdle();

        GameObject canvasGo = new GameObject("SpoonCursorCanvas", typeof(RectTransform));
        canvasGo.transform.SetParent(transform, false);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject spoonGo = new GameObject("Spoon", typeof(RectTransform));
        spoonGo.transform.SetParent(canvasGo.transform, false);
        spoonRect = (RectTransform)spoonGo.transform;
        spoonRect.anchorMin = spoonRect.anchorMax = Vector2.zero;
        spoonRect.pivot = new Vector2(0.5f, 0f); // 指標位置 = 湯匙底部 = 落地點
        spoonRect.sizeDelta = size;
        spoonImage = spoonGo.AddComponent<Image>();
        spoonImage.raycastTarget = false;
        spoonImage.preserveAspect = true;
        spoonImage.enabled = false;
    }

    private void Update()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        if (!isFalling)
        {
            spoonRect.position = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame && fallController != null)
            {
                StartFall();
            }
            return;
        }

        // normalizedTime 已包含 state speed 與 clip 長度，動畫加速時落地判定也跟著提早
        float progress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (!impacted && progress >= impactPoint)
        {
            impacted = true;
            if (gooseParade != null)
            {
                gooseParade.KillAt(spoonRect.position);
            }
        }
        if (progress >= 1f)
        {
            ResetToIdle();
        }
    }

    private void LateUpdate()
    {
        // Animator 在 Update 之後、LateUpdate 之前套用動畫，這時 sprite 已是本幀的畫格
        spoonImage.sprite = spriteSource.sprite;
        spoonImage.enabled = spoonImage.sprite != null && Pointer.current != null;
    }

    private void StartFall()
    {
        isFalling = true;
        impacted = false;
        animator.speed = 1f;
        animator.Play(0, 0, 0f);
    }

    // 停在第一格（直立的湯匙）
    private void ResetToIdle()
    {
        isFalling = false;
        animator.speed = 0f;
        animator.Play(0, 0, 0f);
    }
}

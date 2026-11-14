using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TriInspector;

// Author : Gazza Liu <gazzaliu@unizzagames.com>

/// <summary>
/// 通用 Transform 動畫元件: 掛在任意物件上, 依設定讓該物件位移 / 旋轉 / 縮放至指定目標
/// 三個通道 (Move / Rotate / Scale) 可各自獨立開關與設定, 底層統一使用 LeanTween
/// Rotate 通道支援 TweenTo (補間到角度) 與 Spin (持續自轉, 可帶變速) 兩種模式
/// 正向播放 (Play) 會先歸位到'起始狀態'再往目標跑, 每次結果一致 (慣例同 MaskController)
/// 註: Inspector 上的 ContextMenu 預覽需在 Play Mode 才會有效果 (LeanTween 需要 Update 迴圈)
/// </summary>
public class TransformAnimator : MonoBehaviour
{
    public enum AutoPlay
    {
        None,     // 不自動播放, 需手動呼叫 Play()
        OnStart,  // Start 時播放
        OnEnable  // 每次 OnEnable 時播放
    }

    public enum LoopType
    {
        Once,     // 播放一次
        PingPong, // 來回 (A->B->A->B...)
        Repeat    // 重複 (每次從起點重來 A->B, A->B...)
    }

    public enum ValueMode
    {
        Absolute, // 直接指定最終數值
        Relative  // 相對於起始值的偏移 (位移/旋轉為相加, 縮放為逐分量相乘)
    }

    public enum CoordSpace
    {
        World, // 世界座標
        Local  // 區域座標 (相對父物件)
    }

    public enum RotateType
    {
        TweenTo, // 補間到指定角度 (有目標, 受 loopType 影響)
        Spin     // 持續自轉 (無目標, 可帶變速, 不受 loopType 影響)
    }

    [Header("Playback")]
    [Tooltip("何時自動播放")]
    public AutoPlay autoPlay = AutoPlay.OnEnable;

    [Tooltip("循環方式")]
    public LoopType loopType = LoopType.Once;

    [Tooltip("循環次數, <= 0 為無限循環 (僅 PingPong / Repeat 有效)")]
    [ShowIf(nameof(IsLooping))]
    public int loopCount = -1;

    [Tooltip("使用不受 TimeScale 影響的時間 (暫停選單背景等場合)")]
    public bool useUnscaledTime = false;

    [Tooltip("停用物件時是否自動歸位到起始狀態 (循環動畫建議勾, 一次性入場動畫建議不勾)")]
    public bool resetOnDisable = false;

    [Tooltip("整段動畫播完時觸發 (僅 Once 或有限循環會觸發, 無限循環 / Spin 不會)")]
    public UnityEvent onComplete;

    // ----- Move (位移) -----
    [Header("Move Settings")]
    public bool enableMove = false;

    [ShowIf(nameof(enableMove))]
    [Tooltip("Absolute: 移動到指定座標 / Relative: 相對起始位置的偏移量")]
    public ValueMode moveMode = ValueMode.Relative;

    [ShowIf(nameof(enableMove))]
    [Tooltip("使用世界或區域座標")]
    public CoordSpace moveSpace = CoordSpace.Local;

    [ShowIf(nameof(enableMove))]
    [Tooltip("目標位置 (Absolute) 或偏移量 (Relative)")]
    public Vector3 moveTarget = Vector3.zero;

    [ShowIf(nameof(enableMove))]
    [Tooltip("位移所需時間 (秒)")]
    public float moveDuration = 1f;

    [ShowIf(nameof(enableMove))]
    [Tooltip("位移開始前的延遲 (秒)")]
    public float moveDelay = 0f;

    [ShowIf(nameof(enableMove))]
    public LeanTweenType moveEase = LeanTweenType.easeInOutSine;

    // ----- Rotate (旋轉) -----
    [Header("Rotate Settings")]
    public bool enableRotate = false;

    [ShowIf(nameof(enableRotate))]
    [Tooltip("TweenTo: 旋轉到指定角度 / Spin: 持續自轉 (可帶速度變化)")]
    public RotateType rotateType = RotateType.TweenTo;

    // ----- TweenTo 模式 -----
    [ShowIf(nameof(IsRotateTween))]
    [Tooltip("Absolute: 旋轉到指定角度 / Relative: 相對起始角度的增量")]
    public ValueMode rotateMode = ValueMode.Relative;

    [ShowIf(nameof(IsRotateTween))]
    [Tooltip("使用世界或區域座標")]
    public CoordSpace rotateSpace = CoordSpace.Local;

    [ShowIf(nameof(IsRotateTween))]
    [Tooltip("目標歐拉角 (Absolute) 或增量 (Relative)2D 通常只用 Z, 例如 (0,0,360) 轉一圈")]
    public Vector3 rotateTarget = new Vector3(0f, 0f, 360f);

    [ShowIf(nameof(IsRotateTween))]
    [Tooltip("旋轉所需時間 (秒)")]
    public float rotateDuration = 1f;

    [ShowIf(nameof(IsRotateTween))]
    [Tooltip("旋轉開始前的延遲 (秒)")]
    public float rotateDelay = 0f;

    [ShowIf(nameof(IsRotateTween))]
    public LeanTweenType rotateEase = LeanTweenType.easeInOutSine;

    // ----- Spin 模式 -----
    [ShowIf(nameof(IsRotateSpin))]
    [Tooltip("自轉軸, 2D 用 Z 即 (0,0,1)")]
    public Vector3 spinAxis = new Vector3(0f, 0f, 1f);

    [ShowIf(nameof(IsRotateSpin))]
    [Tooltip("自轉速度 (度/秒), 可為負值反向")]
    public float spinSpeed = 100f;

    [ShowIf(nameof(IsRotateSpin))]
    [Tooltip("速度變化量 (度/秒), 0 為等速; 大於 0 時速度會在 speed +- variation 之間來回")]
    public float spinVariation = 0f;

    [ShowIf(nameof(IsRotateSpin))]
    [Tooltip("速度來回變化的週期 (秒)")]
    public float spinPeriod = 4f;

    [ShowIf(nameof(IsRotateSpin))]
    public LeanTweenType spinEase = LeanTweenType.easeInOutSine;

    // ----- Scale (縮放) -----
    [Header("Scale Settings")]
    public bool enableScale = false;

    [ShowIf(nameof(enableScale))]
    [Tooltip("Absolute: 縮放到指定值 / Relative: 相對起始縮放的倍率 (逐分量相乘)")]
    public ValueMode scaleMode = ValueMode.Absolute;

    [ShowIf(nameof(enableScale))]
    [Tooltip("目標縮放 (localScale)")]
    public Vector3 scaleTarget = Vector3.one;

    [ShowIf(nameof(enableScale))]
    [Tooltip("縮放所需時間 (秒)")]
    public float scaleDuration = 1f;

    [ShowIf(nameof(enableScale))]
    [Tooltip("縮放開始前的延遲 (秒)")]
    public float scaleDelay = 0f;

    [ShowIf(nameof(enableScale))]
    public LeanTweenType scaleEase = LeanTweenType.easeInOutSine;

    // ----- Runtime -----
    private Vector3 _startWorldPos, _startLocalPos;
    private Vector3 _startWorldEuler, _startLocalEuler;
    private Vector3 _startScale;
    private bool _captured;

    private readonly List<int> _tweenIds = new List<int>();
    private int _pending; // 尚未結束的通道數
    private Action _runtimeOnComplete; // Play() 傳入的一次性回呼

    private float _curSpinSpeed; // Spin 當前速度 (度/秒)
    private bool _isSpinning;

    // TriInspector
    private bool IsLooping => loopType != LoopType.Once;
    private bool IsRotateTween => enableRotate && rotateType == RotateType.TweenTo;
    private bool IsRotateSpin => enableRotate && rotateType == RotateType.Spin;

    void Awake()
    {
        CaptureStart();
    }

    void Start()
    {
        if (autoPlay == AutoPlay.OnStart) Play();
    }

    void OnEnable()
    {
        if (autoPlay == AutoPlay.OnEnable) Play();
    }

    void OnDisable()
    {
        Stop();
        if (resetOnDisable) ResetToStart();
    }

    void OnDestroy()
    {
        Stop();
    }

    void Update()
    {
        if (!_isSpinning) return;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        transform.Rotate(spinAxis * (_curSpinSpeed * dt), Space.Self);
    }

    /// <summary>記錄目前 Transform 為'起始狀態', 供 Relative 計算與 PlayReverse / ResetToStart 使用</summary>
    public void CaptureStart()
    {
        _startWorldPos = transform.position;
        _startLocalPos = transform.localPosition;
        _startWorldEuler = transform.eulerAngles;
        _startLocalEuler = transform.localEulerAngles;
        _startScale = transform.localScale;
        _captured = true;
    }

    /// <summary>從起始狀態往目標播放</summary>
    public void Play(Action onCompleteCallback = null)
    {
        PlayInternal(reverse: false, onCompleteCallback);
    }

    /// <summary>反向播放: 從目前狀態回到起始狀態 (Spin 模式不受反向影響, 照常自轉)</summary>
    public void PlayReverse(Action onCompleteCallback = null)
    {
        PlayInternal(reverse: true, onCompleteCallback);
    }

    /// <summary>停止播放 (只取消本元件建立的 tween 與自轉, 不影響同物件上其他 tween)</summary>
    public void Stop()
    {
        for (int i = 0; i < _tweenIds.Count; i++)
        {
            LeanTween.cancel(gameObject, _tweenIds[i]);
        }
        _tweenIds.Clear();
        _pending = 0;
        _isSpinning = false;
        _runtimeOnComplete = null;
    }

    /// <summary>立即停止並把 Transform 還原為起始狀態</summary>
    public void ResetToStart()
    {
        Stop();
        if (!_captured) return;
        transform.localPosition = _startLocalPos;
        transform.localEulerAngles = _startLocalEuler;
        transform.localScale = _startScale;
    }

    // ----- Internal -----
    private void PlayInternal(bool reverse, Action onCompleteCallback)
    {
        if (!_captured) CaptureStart();

        Stop(); // 清掉舊的 tween 與計數

        // 正向播放先歸位到起始狀態, 確保每次播放結果一致 (慣例同 MaskController)
        if (!reverse) SnapEnabledToStart();

        _runtimeOnComplete = onCompleteCallback;
        _pending = 0;

        if (enableMove) StartMove(reverse);
        if (enableRotate) StartRotate(reverse);
        if (enableScale) StartScale(reverse);

        // 沒有任何'會結束的'通道時直接視為完成 (Spin 為無限, 不算)
        if (_pending == 0 && !_isSpinning) FireComplete();
    }

    private void SnapEnabledToStart()
    {
        if (enableMove)
        {
            if (moveSpace == CoordSpace.World) transform.position = _startWorldPos;
            else transform.localPosition = _startLocalPos;
        }
        // Spin 模式不歸位角度 (從當前朝向繼續轉); TweenTo 模式才歸位
        if (enableRotate && rotateType == RotateType.TweenTo)
        {
            if (rotateSpace == CoordSpace.World) transform.eulerAngles = _startWorldEuler;
            else transform.localEulerAngles = _startLocalEuler;
        }
        if (enableScale)
        {
            transform.localScale = _startScale;
        }
    }

    private void StartMove(bool reverse)
    {
        bool useWorld = moveSpace == CoordSpace.World;
        Vector3 baseVal = useWorld ? _startWorldPos : _startLocalPos;
        Vector3 target = reverse
            ? baseVal
            : (moveMode == ValueMode.Absolute ? moveTarget : baseVal + moveTarget);

        LTDescr d = useWorld
            ? LeanTween.move(gameObject, target, moveDuration)
            : LeanTween.moveLocal(gameObject, target, moveDuration);

        Register(d, moveDelay, moveEase);
    }

    private void StartRotate(bool reverse)
    {
        if (rotateType == RotateType.Spin)
        {
            StartSpin();
            return;
        }

        bool useWorld = rotateSpace == CoordSpace.World;
        Vector3 baseVal = useWorld ? _startWorldEuler : _startLocalEuler;
        Vector3 target = reverse
            ? baseVal
            : (rotateMode == ValueMode.Absolute ? rotateTarget : baseVal + rotateTarget);

        LTDescr d = useWorld
            ? LeanTween.rotate(gameObject, target, rotateDuration)
            : LeanTween.rotateLocal(gameObject, target, rotateDuration);

        Register(d, rotateDelay, rotateEase);
    }

    private void StartSpin()
    {
        _isSpinning = true;

        if (Mathf.Abs(spinVariation) > 0.001f)
        {
            float minS = spinSpeed - spinVariation;
            float maxS = spinSpeed + spinVariation;

            // 避免速度跨越 0 導致忽正忽反 (與 ImageTransformObject 一致)
            if (spinSpeed > 0f) minS = Mathf.Max(0.1f, minS);
            else if (spinSpeed < 0f) maxS = Mathf.Min(-0.1f, maxS);

            LTDescr d = LeanTween.value(gameObject, minS, maxS, spinPeriod)
                .setEase(spinEase)
                .setLoopPingPong()
                .setOnUpdate((float v) => _curSpinSpeed = v);

            if (useUnscaledTime) d.setIgnoreTimeScale(true);
            _tweenIds.Add(d.id); // 只為了 Stop 時能取消, 不計入 _pending
        }
        else
        {
            _curSpinSpeed = spinSpeed;
        }
        // Spin 為無限, 不計入 _pending, 也不觸發 onComplete
    }

    private void StartScale(bool reverse)
    {
        Vector3 target;
        if (reverse)
        {
            target = _startScale;
        }
        else if (scaleMode == ValueMode.Absolute)
        {
            target = scaleTarget;
        }
        else // Relative: 逐分量相乘
        {
            target = new Vector3(
                _startScale.x * scaleTarget.x,
                _startScale.y * scaleTarget.y,
                _startScale.z * scaleTarget.z);
        }

        LTDescr d = LeanTween.scale(gameObject, target, scaleDuration);
        Register(d, scaleDelay, scaleEase);
    }

    private void Register(LTDescr d, float delay, LeanTweenType ease)
    {
        d.setEase(ease);
        if (delay > 0f) d.setDelay(delay);
        if (useUnscaledTime) d.setIgnoreTimeScale(true);

        ApplyLoop(d);

        _pending++;
        d.setOnComplete(OnChannelComplete); // 無限循環永遠不會觸發, 有限循環會在跑完後觸發

        _tweenIds.Add(d.id);
    }

    private void ApplyLoop(LTDescr d)
    {
        switch (loopType)
        {
            case LoopType.PingPong:
                if (loopCount <= 0) d.setLoopPingPong();
                else d.setLoopPingPong(loopCount);
                break;

            case LoopType.Repeat:
                if (loopCount <= 0) d.setLoopClamp();
                else d.setLoopClamp(loopCount);
                break;

            case LoopType.Once:
            default:
                break;
        }
    }

    private void OnChannelComplete()
    {
        _pending--;
        if (_pending <= 0) FireComplete();
    }

    private void FireComplete()
    {
        _pending = 0;
        // 保留 _tweenIds 中的 Spin value-tween id (Spin 仍在跑), 只清理已結束的補間交給 LeanTween 自身
        Action cb = _runtimeOnComplete;
        _runtimeOnComplete = null;
        cb?.Invoke();
        onComplete?.Invoke();
    }

    // ----- Editor Preview -----
    [ContextMenu("Play (需 Play Mode)")]
    private void EditorPlay()
    {
        CaptureStart();
        Play();
    }

    [ContextMenu("Reset To Start")]
    private void EditorReset()
    {
        ResetToStart();
    }
}

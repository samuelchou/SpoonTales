using SerialSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Keyboard stand-in for the Arduino "球門" input:
// QWER = 黑球 (Black) at lane 0..3, ASDF = 藍球 (Blue), ZXCV = 白球 (White)
public class LaneShooter : MonoBehaviour
{
    private readonly List<(Key key, SpoonColor sc, int idx)> mappings = new List<(Key, SpoonColor, int)>()
    {
        (Key.Q, SpoonColor.Black, 0),
        (Key.W, SpoonColor.Black, 1),
        (Key.E, SpoonColor.Black, 2),
        (Key.R, SpoonColor.Black, 3),
        (Key.A, SpoonColor.Blue, 0),
        (Key.S, SpoonColor.Blue, 1),
        (Key.D, SpoonColor.Blue, 2),
        (Key.F, SpoonColor.Blue, 3),
        (Key.Z, SpoonColor.White, 0),
        (Key.X, SpoonColor.White, 1),
        (Key.C, SpoonColor.White, 2),
        (Key.V, SpoonColor.White, 3),
    };
    [Header("General")]
    [Tooltip("找不到 Main Camera 時的備援生成 Z；正常情況由攝影機視錐動態計算")]
    [SerializeField]
    private float _spawnZ = 1f;
    [Tooltip("球生成在畫面下緣之外多遠（viewport 單位，0.15 = 螢幕高度的 15%）")]
    [SerializeField]
    private float _offscreenMargin = 0.15f;
    [SerializeField]
    private float _ballScale = 0.35f;
    [Header("Ball")]
    [Tooltip("球的 prefab（需有 Collider，會自動補上 Rigidbody / Ball）；留空則用 Sphere 佔位，並依顏色上色")]
    [SerializeField]
    private GameObject _ballPrefab;
    [Tooltip("球速（units/sec）")]
    [SerializeField]
    private float _ballSpeed = 10f;
    [Tooltip("球的存活秒數；只是保險，要夠長讓球能飛到遠方的 KillZone（Z≈45）")]
    [SerializeField]
    private float _ballLifetime = 6f;
    // 佔位球的共用材質（每種顏色一份），避免每顆球都複製一份 material
    private readonly Dictionary<SpoonColor, Material> _ballMaterials = new Dictionary<SpoonColor, Material>();
    [Header("Spoon Fall")]
    [Tooltip("發射黑球時，在發射位置播放這個湯匙倒下動畫；留空則不播放")]
    [SerializeField]
    private RuntimeAnimatorController _spoonFallController;
    [Tooltip("湯匙出現在畫面上的高度（viewport 單位，0 = 螢幕下緣）；球是從畫面外發射，所以放在下緣內側才看得到")]
    [SerializeField]
    private float _spoonFallViewportY = 0.12f;
    [SerializeField]
    private float _spoonFallScale = 1f;
    [Header("Keyboard")]
    [SerializeField]
    private bool _isEnableKb = true;
    [Header("Serial")]
    [SerializeField]
    private bool _isEnableSerial = false;
    [SerializeField]
    private int _baudRate = 9600;
    [SerializeField]
    private int _thMm = 50;
    private List<bool> _toggleSerials = new List<bool>();

    private void Start()
    {
        if (_isEnableSerial)
        {
            SerialConnector.Instance.AutoConnectArduinoNano(_baudRate);
            _toggleSerials.Clear();
            foreach(var _ in mappings)
            {
                _toggleSerials.Add(false);
            }
        }
    }

    private void Update()
    {
        if (RoundManager.Instance == null || !RoundManager.Instance.RoundActive)
        {
            return;
        }

        if (_isEnableKb)
        {
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                for (int i = 0; i < mappings.Count; i++)
                {
                    var (key, color, laneIndex) = mappings[i];
                    CheckKey(kb[key], color, laneIndex);
                }
            }
        }
        if (_isEnableSerial &&
            SerialConnector.Instance.IsConnected &&
            SerialConnector.Instance.TryGetData(out var dataList))
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                var idx = System.Math.Min(i, mappings.Count - 1);
                var (_, color, laneIndex) = mappings[idx];
                if (data < _thMm && !_toggleSerials[idx])
                {
                    Spawn(color, laneIndex);
                    _toggleSerials[idx] = true;
                    Debug.Log($"Serial Spawn: {color} at lane {laneIndex}");
                }
                else if (data >= _thMm && _toggleSerials[idx])
                {
                    _toggleSerials[idx] = false;
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Material mat in _ballMaterials.Values)
        {
            if (mat != null) Destroy(mat);
        }
        _ballMaterials.Clear();

        if (SerialConnector.Instance.IsConnected)
        {
            SerialConnector.Instance.Disconnect();
        }
    }

    private void CheckKey(KeyControl key, SpoonColor color, int laneIndex)
    {
        if (key.wasPressedThisFrame)
        {
            Spawn(color, laneIndex);
        }
    }

    // 沿著畫面下緣外側（_offscreenMargin）的視線，找出與球飛行高度（Lanes.Y）相交的 Z，
    // 球從那裡生成就一定在畫面外，再直線飛進畫面。
    private float ComputeOffscreenSpawnZ()
    {
        return ComputeZAtViewportY(-_offscreenMargin);
    }

    private float ComputeZAtViewportY(float viewportY)
    {
        Camera cam = Camera.main;
        if (cam == null) return _spawnZ;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, viewportY, 0f));
        if (Mathf.Abs(ray.direction.y) < 1e-4f) return _spawnZ;

        float t = (Lanes.Y - ray.origin.y) / ray.direction.y;
        return t > 0f ? ray.GetPoint(t).z : _spawnZ;
    }

    private void Spawn(SpoonColor color, int laneIndex)
    {
        Vector3 pos = new Vector3(Lanes.X[laneIndex], Lanes.Y, ComputeOffscreenSpawnZ());

        GameObject go;
        if (_ballPrefab != null)
        {
            go = Instantiate(_ballPrefab, pos, Quaternion.identity);
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetPositionAndRotation(pos, Quaternion.identity);
            Renderer rend = go.GetComponent<Renderer>();
            rend.sharedMaterial = GetBallMaterial(color, rend.sharedMaterial);
        }
        go.name = "Ball_" + color;
        go.transform.localScale = Vector3.one * _ballScale;

        Collider col = go.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Ball ball = go.GetComponent<Ball>();
        if (ball == null) ball = go.AddComponent<Ball>();
        ball.Kind = color;
        ball.Speed = _ballSpeed;
        ball.Lifetime = _ballLifetime;

        AudioManager.Instance?.PlaySfx("shoot_ball");

        if (color == SpoonColor.Black)
        {
            PlaySpoonFall(laneIndex);
        }
    }

    // 在發射的賽道上、畫面下緣內側播放一次 spoon-fall，播完就銷毀
    private void PlaySpoonFall(int laneIndex)
    {
        if (_spoonFallController == null) return;

        Vector3 pos = new Vector3(Lanes.X[laneIndex], Lanes.Y, ComputeZAtViewportY(_spoonFallViewportY));
        GameObject go = new GameObject("SpoonFall");
        go.transform.SetPositionAndRotation(pos, Quaternion.identity);
        go.transform.localScale = Vector3.one * _spoonFallScale;
        go.AddComponent<SpriteRenderer>();
        Animator animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = _spoonFallController;

        float length = 0f;
        foreach (AnimationClip clip in _spoonFallController.animationClips)
        {
            length = Mathf.Max(length, clip.length);
        }
        // clip 是 loop 的，提早一點點銷毀，避免最後一幀繞回直立的第一格
        Destroy(go, Mathf.Max(0f, length - 0.01f));
    }

    private Material GetBallMaterial(SpoonColor color, Material source)
    {
        if (!_ballMaterials.TryGetValue(color, out Material mat) || mat == null)
        {
            mat = new Material(source) { name = "BallMat_" + color, color = ColorFor(color) };
            _ballMaterials[color] = mat;
        }
        return mat;
    }

    private static Color ColorFor(SpoonColor c)
    {
        switch (c)
        {
            case SpoonColor.Black: return Color.black;
            case SpoonColor.Blue: return Color.blue;
            case SpoonColor.White: return Color.white;
            default: return Color.gray;
        }
    }
}

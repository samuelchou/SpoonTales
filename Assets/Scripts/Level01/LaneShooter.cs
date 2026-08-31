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
    [SerializeField]
    private float _spawnZ = 1f;
    [SerializeField]
    private float _ballScale = 0.35f;
    [Header("Keyboard")]
    [SerializeField]
    private bool _isEnableKb = true;
    [Header("Serial")]
    [SerializeField]
    private bool _isEnableSerial = false;
    [SerializeField]
    private string _port = "COM3";
    [SerializeField]
    private int _baudRate = 9600;
    [SerializeField]
    private int _thMm = 50;

    private void Start()
    {
        if (_isEnableSerial)
        {
            SerialConnector.Instance.Connect(_port, _baudRate);
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
                if (data < _thMm)
                {
                    Spawn(color, laneIndex);
                }
            }
        }
    }

    private void OnDestroy()
    {
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

    private void Spawn(SpoonColor color, int laneIndex)
    {
        Vector3 pos = new Vector3(Lanes.X[laneIndex], Lanes.Y, _spawnZ);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Ball_" + color;
        go.transform.SetPositionAndRotation(pos, Quaternion.identity);
        go.transform.localScale = Vector3.one * _ballScale;
        go.GetComponent<Renderer>().material.color = ColorFor(color);

        Collider col = go.GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Ball ball = go.AddComponent<Ball>();
        ball.Kind = color;
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

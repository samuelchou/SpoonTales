using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Keyboard stand-in for the Arduino "球門" input:
// QWER = 黑球 (Black) at lane 0..3, ASDF = 藍球 (Blue), ZXCV = 白球 (White)
public class LaneShooter : MonoBehaviour
{
    [SerializeField] private float spawnZ = 1f;
    [SerializeField] private float ballScale = 0.35f;

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || RoundManager.Instance == null || !RoundManager.Instance.RoundActive)
        {
            return;
        }

        CheckKey(kb.qKey, SpoonColor.Black, 0);
        CheckKey(kb.wKey, SpoonColor.Black, 1);
        CheckKey(kb.eKey, SpoonColor.Black, 2);
        CheckKey(kb.rKey, SpoonColor.Black, 3);

        CheckKey(kb.aKey, SpoonColor.Blue, 0);
        CheckKey(kb.sKey, SpoonColor.Blue, 1);
        CheckKey(kb.dKey, SpoonColor.Blue, 2);
        CheckKey(kb.fKey, SpoonColor.Blue, 3);

        CheckKey(kb.zKey, SpoonColor.White, 0);
        CheckKey(kb.xKey, SpoonColor.White, 1);
        CheckKey(kb.cKey, SpoonColor.White, 2);
        CheckKey(kb.vKey, SpoonColor.White, 3);
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
        Vector3 pos = new Vector3(Lanes.X[laneIndex], Lanes.Y, spawnZ);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Ball_" + color;
        go.transform.SetPositionAndRotation(pos, Quaternion.identity);
        go.transform.localScale = Vector3.one * ballScale;
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

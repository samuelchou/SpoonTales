using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class BallThrower : MonoBehaviour
{
    [SerializeField] private float throwSpeed = 12f;
    [SerializeField] private float ballLifetime = 3f;
    [SerializeField] private float ballRadius = 0.15f;
    [SerializeField] private float spawnDistance = 0.8f;
    [SerializeField] private Color ballColor = new Color(0.85f, 0.2f, 0.2f);

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ThrowBall();
        }
    }

    private void ThrowBall()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPos);
        Vector3 direction = ray.direction.normalized;
        Vector3 spawnPos = ray.origin + direction * spawnDistance;

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "ThrownBall";
        ball.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        ball.transform.localScale = Vector3.one * (ballRadius * 2f);
        ball.GetComponent<Renderer>().material.color = ballColor;

        Rigidbody rb = ball.AddComponent<Rigidbody>();
        rb.linearVelocity = direction * throwSpeed;

        Destroy(ball, ballLifetime);
    }
}

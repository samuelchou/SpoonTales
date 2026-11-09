using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    public SpoonColor Kind;

    [Tooltip("前進速度（units/sec）。由 LaneShooter 生成時覆寫")]
    public float Speed = 10f;
    [Tooltip("存活秒數，僅作為保險；正常情況由 KillZone 銷毀。由 LaneShooter 生成時覆寫")]
    public float Lifetime = 6f;

    private void Start()
    {
        Destroy(gameObject, Lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.forward * (Speed * Time.deltaTime);
    }
}

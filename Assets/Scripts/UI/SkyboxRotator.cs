using UnityEngine;

// 讓天空盒緩慢旋轉（需使用有 _Rotation 屬性的天空盒，例如 Skybox/Panoramic）。
public class SkyboxRotator : MonoBehaviour
{
    [Tooltip("每秒旋轉角度；負值反向")]
    [SerializeField] private float degreesPerSecond = 1.5f;

    private static readonly int RotationId = Shader.PropertyToID("_Rotation");

    private Material skybox;
    private float rotation;

    private void Start()
    {
        if (RenderSettings.skybox == null || !RenderSettings.skybox.HasProperty(RotationId))
        {
            enabled = false;
            return;
        }

        // 複製一份材質再改，避免在編輯器 Play 時改到專案裡的 .mat 檔
        skybox = new Material(RenderSettings.skybox);
        RenderSettings.skybox = skybox;
        rotation = skybox.GetFloat(RotationId);
    }

    private void Update()
    {
        rotation = Mathf.Repeat(rotation + degreesPerSecond * Time.unscaledDeltaTime, 360f);
        skybox.SetFloat(RotationId, rotation);
    }

    private void OnDestroy()
    {
        if (skybox != null)
        {
            Destroy(skybox);
        }
    }
}

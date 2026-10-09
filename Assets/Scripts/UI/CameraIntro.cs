using System.Collections;
using UnityEngine;

// 進入場景時的開場運鏡：攝影機從較遠處平滑推進到場景中擺好的位置（場景裡的 Transform 就是終點）。
public class CameraIntro : MonoBehaviour
{
    [Tooltip("起點相對於終點的位置偏移（世界座標）")]
    [SerializeField] private Vector3 startOffset = new Vector3(0f, 2f, -10f);
    [Tooltip("起點相對於終點的角度偏移（Euler 角度）")]
    [SerializeField] private Vector3 startRotationOffset = new Vector3(6f, 0f, 0f);
    [SerializeField] private float duration = 2.5f;

    private IEnumerator Start()
    {
        Vector3 endPos = transform.position;
        Quaternion endRot = transform.rotation;
        Vector3 startPos = endPos + startOffset;
        Quaternion startRot = endRot * Quaternion.Euler(startRotationOffset);

        // 用 unscaledDeltaTime，避免從暫停狀態回到主選單時 timeScale 為 0 導致運鏡卡住
        float t = 0f;
        while (t < duration)
        {
            float k = 1f - Mathf.Pow(1f - t / duration, 3f); // ease-out cubic
            transform.SetPositionAndRotation(Vector3.Lerp(startPos, endPos, k), Quaternion.Slerp(startRot, endRot, k));
            yield return null;
            t += Time.unscaledDeltaTime;
        }
        transform.SetPositionAndRotation(endPos, endRot);
    }
}

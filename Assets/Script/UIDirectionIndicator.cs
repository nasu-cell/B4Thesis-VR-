using UnityEngine;

public class UIDirectionIndicator : MonoBehaviour
{
    public CheckPointManager manager;
    public RectTransform arrowTransform;
    public Transform vrCamera;

    void Update()
    {
        // --- 修正ポイント：15行目付近のエラーを防ぐための徹底チェック ---
        if (manager == null) return;
        if (arrowTransform == null) return;
        if (manager.CurrentTarget == null) return; // ←ここでターゲットがいない時は処理を抜ける
        
        // カメラの指定が空なら Camera.main を使う
        Transform cam = (vrCamera != null) ? vrCamera : (Camera.main != null ? Camera.main.transform : null);
        if (cam == null) return;

        // --- ここから計算 ---
        Vector3 targetPos = manager.CurrentTarget.position;
        Vector3 localTargetPos = cam.InverseTransformPoint(targetPos);
        
        float angle = Mathf.Atan2(localTargetPos.x, localTargetPos.z) * Mathf.Rad2Deg;

        // 矢印を回転
        arrowTransform.localRotation = Quaternion.Euler(0, 0, -angle);
    }
}
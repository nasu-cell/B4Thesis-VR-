using UnityEngine;

public class NeckSnapTurn : MonoBehaviour
{
    public Transform cameraTransform; // Main Cameraを割り当て
    public float thresholdAngle = 60f; // 回転を開始する首の角度
    public float turnAngle = 30f;      // 一回の回転量
    public float turnInterval = 0.5f;  // 回転の間隔（秒）

    private float timer = 0f;

    void Update()
    {
        // カメラの「正面方向」とXR Originの「正面方向」のズレ（首の角度）を計算
        float horizontalAngle = cameraTransform.localEulerAngles.y;
        if (horizontalAngle > 180) horizontalAngle -= 360;

        // 右に限界まで向いている場合
        if (horizontalAngle > thresholdAngle)
        {
            ExecuteTurn(turnAngle);
        }
        // 左に限界まで向いている場合
        else if (horizontalAngle < -thresholdAngle)
        {
            ExecuteTurn(-turnAngle);
        }
        else
        {
            timer = 0f; // 正面を向いたらタイマーリセット
        }
    }

    void ExecuteTurn(float angle)
    {
        timer += Time.deltaTime;
        if (timer >= turnInterval)
        {
            // XR Origin自体を回転させる
            transform.Rotate(0, angle, 0);
            timer = 0f;
        }
    }
}
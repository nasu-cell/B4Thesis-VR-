using UnityEngine;

public class CameraMoveByPitch : MonoBehaviour
{
    [Header("参照コンポーネント")]
    public NetworkCoordinateClient networkClient;
    public Transform cameraTransform;

    [Header("しきい値設定 (ペダルの可動域)")]
    public float upperThreshold;
    public float lowerThreshold;

    [Header("速度・ピッチ設定")]
    public float maxMoveSpeed;
    public float targetRPM;
    public float lerpSpeed;
    [Tooltip("足を止めてから、あるいはロストし続けてから停止するまでの猶予時間")]
    public float stopTimeout;

    [Header("デバッグ情報")]
    public float currentRPM = 0f;
    public float actualSpeed = 0f;
    public bool isUpSide = false;

    private float lastStateChangeTime = 0f;
    private float targetSpeed = 0f;
    private float lastDataUpdateTime = 0f;

    void Update()
    {
        if (networkClient == null || cameraTransform == null) return;

        // 1. マーカーが見えている時だけ周期を更新する
        if (networkClient.isMarkerVisible)
        {
            float currentY = networkClient.cordinate_R;

            if (!isUpSide && currentY > upperThreshold)
            {
                CalculateInterval();
                isUpSide = true;
            }
            else if (isUpSide && currentY < lowerThreshold)
            {
                CalculateInterval();
                isUpSide = false;
            }
        }

        // 2. 停止判定 (最後に「正常な一漕ぎ」を検知してから stopTimeout 秒経ったか)
        // ロストしている間はこの判定が「キープ」として働き、時間が過ぎると停止する
        if (Time.time - lastDataUpdateTime > stopTimeout)
        {
            targetSpeed = 0f;
            currentRPM = 0f;
        }

        // 3. 速度の計算と適用
        actualSpeed = Mathf.Lerp(actualSpeed, targetSpeed, Time.deltaTime * lerpSpeed);

        if (actualSpeed > 0.01f)
        {
            Vector3 moveDirection = cameraTransform.forward;
            moveDirection.y = 0;
            moveDirection.Normalize();
            transform.position += moveDirection * actualSpeed * Time.deltaTime;
        }
    }

    private void CalculateInterval()
    {
        float currentTime = Time.time;
        float interval = currentTime - lastStateChangeTime;

        if (interval > 0.1f) 
        {
            float fullCycleTime = interval * 2f;
            currentRPM = 60f / fullCycleTime;

            float speedFactor = Mathf.Clamp01(currentRPM / targetRPM);
            targetSpeed = speedFactor * maxMoveSpeed;

            lastStateChangeTime = currentTime;
            lastDataUpdateTime = currentTime; // 正常な入力を受け取った時間を更新
        }
    }
}
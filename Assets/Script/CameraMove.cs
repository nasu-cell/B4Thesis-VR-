using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // 移動速度
    public float moveSpeed = 5.0f; 
    // 回転速度（度/秒）
    public float rotateSpeed = 100.0f; 

    // Updateはフレームごとに1回呼び出されます
    void Update()
    {
        // --- 1. 前後移動 (Wキー: 前進, Sキー: 後退) ---
        
        // 垂直方向の入力（W/Sキー）を取得します。Wキーで正の値(1)、Sキーで負の値(-1)が得られます。
        float verticalInput = Input.GetAxis("Vertical"); 

        // 前進/後退の方向ベクトルを計算します。
        // transform.forwardは、カメラ（オブジェクト）が向いている方向（青軸/Z軸）を示します。
        Vector3 moveDirection = transform.forward * verticalInput;

        // 現在の位置に移動量を加算して移動させます。Time.deltaTimeを乗算することで、
        // フレームレートに依存しないスムーズな移動になります。
        transform.position += moveDirection * moveSpeed * Time.deltaTime;


        // --- 2. 左右回転 (Aキー: 左回転, Dキー: 右回転) ---
        
        // 水平方向の入力（A/Dキー）を取得します。Aキーで負の値(-1)、Dキーで正の値(1)が得られます。
        float horizontalInput = Input.GetAxis("Horizontal");

        // Y軸周りの回転角度を計算します。
        // horizontalInputは回転方向、rotateSpeedは回転の速さ、Time.deltaTimeはスムーズ化に使われます。
        float rotationAmount = horizontalInput * rotateSpeed * Time.deltaTime;

        // 現在の回転に、Y軸周りの新しい回転量を加えます。
        // オイラー角(x, y, z)で回転量を指定します。
        transform.Rotate(0, rotationAmount, 0); 
    }
}
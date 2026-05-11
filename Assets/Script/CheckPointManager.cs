using UnityEngine;
using System.Collections.Generic;
using TMPro; // テキスト表示に必要
using System.Collections; // コルーチン（待機処理）に必要

public class CheckPointManager : MonoBehaviour
{
    public List<Transform> checkPoints; 
    private int currentIndex = 0;

    [Header("ゴール演出設定")]
    public TextMeshProUGUI goalText; // 「ゴール！！」と出すテキスト

    public Transform CurrentTarget => (currentIndex < checkPoints.Count) ? checkPoints[currentIndex] : null;

    void Start()
    {
        // 最初はテキストを非表示にする
        if (goalText != null) goalText.gameObject.SetActive(false);

        foreach (Transform cp in checkPoints)
        {
            if (cp != null) cp.gameObject.SetActive(false);
        }
        ShowNextPoint();
    }

    public void ReachedCheckPoint(int id)
    {
        if (id == currentIndex)
        {
            Debug.Log($"チェックポイント {id} を通過！");
            
            // 通過したポイントを非表示にする
            checkPoints[currentIndex].gameObject.SetActive(false);
            
            currentIndex++;

            if (currentIndex < checkPoints.Count)
            {
                ShowNextPoint();
            }
            else
            {
                // すべてのゴールに到達した時の処理
                StartCoroutine(EndGameSequence());
            }
        }
    }

    // ゴール演出と終了のシーケンス
    IEnumerator EndGameSequence()
    {
        Debug.Log("すべてのゴールに到達しました！");

        // 1. 画面に「ゴール！！」を表示
        if (goalText != null)
        {
            goalText.gameObject.SetActive(true);
        }

        // 2. 3秒間待機（余韻を作る）
        yield return new WaitForSeconds(3f);

        // 3. ゲームを終了
        Debug.Log("ゲームを終了します。");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // エディタ実行中の場合は再生停止
#else
        Application.Quit(); // ビルドしたアプリの場合はアプリ終了
#endif
    }

    void ShowNextPoint()
    {
        if (currentIndex < checkPoints.Count)
        {
            Transform nextPoint = checkPoints[currentIndex];
            if (nextPoint != null)
            {
                nextPoint.gameObject.SetActive(true);
            }
        }
    }
}
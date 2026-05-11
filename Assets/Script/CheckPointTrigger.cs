using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    public int myID; // このチェックポイントの番号（0, 1, 2...）
    private CheckPointManager manager;

    void Start()
    {
        manager = FindFirstObjectByType<CheckPointManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        // プレイヤー（Character Controllerが付いているもの）が触れたら
        if (other.GetComponent<CharacterController>() != null)
        {
            manager.ReachedCheckPoint(myID);
            // 通過したら消える（または見た目を変える）
            gameObject.SetActive(false);
        }
    }
}
using UnityEngine;

public class GoalZone : MonoBehaviour
{
    void Start()
    {
        // このスクリプトを持つGoalZoneが、シーン開始時にデフォルトでアクティブかどうかは、
        // Unityエディタのインスペクターで設定します。
        // レバーで有効化したいステージでは、オブジェクトを非アクティブにしておきます。
        //
        // 以前のコード：
        // // 初期状態ではゴールは常に非表示・非アクティブにしておく
        // // レバーの状態に応じて、LeverControllerのイベント経由で有効化される
        // gameObject.SetActive(false);
    }

    // LeverControllerのUnityEventから呼び出されるメソッド
    public void ActivateGoal()
    {
        Debug.Log("Goal is being activated.");
        gameObject.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーが触れたかタグで確認
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player reached the goal!");
            // GameManagerにステージクリアを通知
            GameManager.Instance.StageClear();
        }
    }
}

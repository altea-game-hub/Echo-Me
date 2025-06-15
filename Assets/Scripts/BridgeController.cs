using UnityEngine;

public class BridgeController : MonoBehaviour
{
    void Start()
    {
        // 初期状態ではブリッジは常に非表示・非アクティブにしておく
        // レバーの状態に応じて、LeverControllerのイベント経由で有効化される
        gameObject.SetActive(false);
    }

    // LeverControllerのUnityEventから呼び出されるメソッド
    public void ActivateBridge()
    {
        Debug.Log("Bridge is being activated.");
        gameObject.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.Tilemaps;

// このスクリプトが必要とするコンポーネントを自動で追加する
[RequireComponent(typeof(TilemapRenderer), typeof(TilemapCollider2D))]
public class TilemapBridgeController : MonoBehaviour
{
    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;

    void Awake()
    {
        // 必要なコンポーネントを取得
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();

        // 初期状態ではブリッジは非表示・当たり判定なし
        tilemapRenderer.enabled = false;
        tilemapCollider.enabled = false;
    }

    // LeverControllerのUnityEventから呼び出されるメソッド
    public void ActivateBridge()
    {
        Debug.Log("Tilemap Bridge is being activated.");
        // 表示と当たり判定を有効にする
        tilemapRenderer.enabled = true;
        tilemapCollider.enabled = true;
    }
}

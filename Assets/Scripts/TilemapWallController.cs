using UnityEngine;
using UnityEngine.Tilemaps;

// このスクリプトが必要とするコンポーネントを自動で追加する
[RequireComponent(typeof(TilemapRenderer), typeof(TilemapCollider2D))]
public class TilemapWallController : MonoBehaviour
{
    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;

    void Awake()
    {
        // 必要なコンポーネントを取得
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    // スイッチなどから呼び出され、壁を非表示・当たり判定なしにする
    public void Deactivate()
    {
        Debug.Log("Tilemap Wall is being deactivated.");
        tilemapRenderer.enabled = false;
        tilemapCollider.enabled = false;
    }

    // スイッチなどから呼び出され、壁を表示・当たり判定ありにする
    public void Activate()
    {
        Debug.Log("Tilemap Wall is being activated.");
        tilemapRenderer.enabled = true;
        tilemapCollider.enabled = true;
    }
}

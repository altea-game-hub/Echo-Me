using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerController))]
public class GhostPlayer : MonoBehaviour
{
    private List<InputState> playbackData;
    private int playbackIndex = 0;
    private bool isPlaying = false;

    private PlayerController controller;

    void Awake()
    {
        // 同じGameObjectにアタッチされているPlayerControllerを取得
        controller = GetComponent<PlayerController>();
    }

    // GameManagerから呼び出される
    public void StartPlayback(List<InputState> recordedData)
    {
        playbackData = recordedData;
        isPlaying = true;
        playbackIndex = 0;

        // ゴーストの外見を設定
        SetupGhostAppearance();
    }

    // 物理演算と同期するため FixedUpdate を使用
    void FixedUpdate()
    {
        if (!isPlaying || playbackData == null || playbackData.Count == 0)
        {
            return;
        }

        // 再生時間が現在の記録のタイムスタンプを超えるまでインデックスを進める
        // (FixedUpdateの実行タイミングの揺らぎを吸収するため)
        float elapsedTime = Time.time - GameManager.Instance.LoopStartTime;
        while (playbackIndex < playbackData.Count - 1 && playbackData[playbackIndex + 1].timeStamp <= elapsedTime)
        {
            playbackIndex++;
        }

        if (playbackIndex < playbackData.Count)
        {
            InputState currentState = playbackData[playbackIndex];
            
            // PlayerControllerに、記録された操作を渡して実行させる
            controller.SetGhostInput(currentState.moveInput, currentState.jumpTriggered, currentState.interactTriggered);
        }
        else
        {
            // 再生が終了
            isPlaying = false;
            // 入力をゼロにして停止させる
            controller.SetGhostInput(Vector2.zero, false, false);
            Debug.Log("Ghost playback finished.");
        }
    }

    private void SetupGhostAppearance()
    {
        // RigidbodyをDynamicにして物理演算に従わせる
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // スプライトを半透明に
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color ghostColor = Color.white;
            ghostColor.a = 0.5f; // 50%透明
            spriteRenderer.color = ghostColor;
        }
    }
}

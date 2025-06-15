using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーが触れたかタグで確認
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered death zone.");
            // GameManagerにループのリセットを依頼
            GameManager.Instance.ResetLoop();
        }
    }
}

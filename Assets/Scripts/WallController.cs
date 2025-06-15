using UnityEngine;

public class WallController : MonoBehaviour
{
    // 壁を非アクティブにする
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    // 壁をアクティブにする（必要に応じて）
    public void Activate()
    {
        gameObject.SetActive(true);
    }
}

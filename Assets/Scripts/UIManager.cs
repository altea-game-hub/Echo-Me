using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject stageSelectUI; // ステージクリア時に表示するUIの親オブジェクト

    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されないようにする
        }

        // ゲーム開始時にはUIを非表示にしておく
        if (stageSelectUI != null)
        {
            stageSelectUI.SetActive(false);
        }
    }

    public void ShowStageSelectUI(bool show)
    {
        if (stageSelectUI != null)
        {
            stageSelectUI.SetActive(show);
        }
    }

    // --- Button Click Handlers ---

    public void GoToNextStage()
    {
        if (GameManager.Instance != null)
        {
            // GameManagerから次のステージ名を取得
            string nextStage = GameManager.Instance.GetNextStageName();
            
            // 次のステージをロード
            GameManager.Instance.LoadStage(nextStage);
        }
    }

    public void ReplayStage()
    {
        if (GameManager.Instance != null)
        {
            // GameManagerから現在のステージ名を取得して、同じステージをリロード
            string currentStage = GameManager.Instance.CurrentSceneName;
            GameManager.Instance.LoadStage(currentStage);
        }
    }
}

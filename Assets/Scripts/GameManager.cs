using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject playerPrefab;
    public GameObject ghostPrefab; // GhostのPrefabをGameManagerが持つ
    public Transform startPoint;

    public float LoopStartTime { get; private set; } // ループ開始時刻を公開

    private GhostRecorder playerRecorder;
    
    [Tooltip("ステージで同時に存在できるゴーストの最大数")]
    public int maxGhostsPerStage = 1; 
    private static List<List<InputState>> ghostRecordings = new List<List<InputState>>(); // 静的にしてシーン間で保持
    public string CurrentSceneName { get; private set; } // 現在のシーン名を保持

    // --- レバー状態管理用の新しい変数 ---
    // 引かれたレバーのIDを永続的に保持する
    private static HashSet<string> pulledLevers = new HashSet<string>();
    // 現在のシーンに存在するレバーを管理する
    private Dictionary<string, LeverController> leversInScene = new Dictionary<string, LeverController>();


    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return; // このインスタンスは不要なので以降の処理をしない
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンをまたいで存在させる
    }

    void Start()
    {
        // シーンがロードされた時に呼ばれるイベントに登録
        SceneManager.sceneLoaded += OnSceneLoaded;

#if UNITY_EDITOR
        // デバッグ用に指定されたシーンがあればそれをロードする
        if (debug_startSceneName != null)
        {
            SceneManager.LoadScene(debug_startSceneName);
            debug_startSceneName = null; // 一度使ったらクリア
            return;
        }
#endif
        // 通常起動時は最初のステージをロード
        SceneManager.LoadScene("SampleScene");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Bootシーンの場合は、ループを開始しない
        if (scene.name == "Boot")
        {
            return;
        }

        // 現在のシーン名を取得
        CurrentSceneName = scene.name;

        // 新しいシーンがロードされたので、シーン内のレバーリストをクリア
        leversInScene.Clear();
        // ゲーム開始時に最初のループを開始
        StartNewLoop();
    }

    void Update()
    {
        // Rキーでのリセット処理を新しいInput Systemの方法で検知
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetLoop();
        }
    }

    private void StartNewLoop()
    {
        // ループ開始時刻を記録
        LoopStartTime = Time.time;

        // startPointを探す（シーン再読み込み後に参照が切れるため）
        startPoint = GameObject.FindWithTag("StartPoint").transform;
        if (startPoint == null)
        {
            Debug.LogError("StartPoint not found in the scene. Please tag your start point object with 'StartPoint'.");
            return;
        }

        // 新しいプレイヤーをスポーン
        GameObject player = Instantiate(playerPrefab, startPoint.position, Quaternion.identity);
        playerRecorder = player.GetComponent<GhostRecorder>();

        // 記録されているすべてのゴーストを再生
        if (ghostRecordings != null)
        {
            foreach (var recording in ghostRecordings)
            {
                if (recording != null && recording.Count > 0)
                {
                    GameObject ghost = Instantiate(ghostPrefab, startPoint.position, Quaternion.identity);
                    GhostPlayer ghostPlayer = ghost.GetComponent<GhostPlayer>();
                    if (ghostPlayer != null)
                    {
                        ghostPlayer.StartPlayback(recording);
                    }
                    else
                    {
                        Debug.LogError("Ghost prefab is missing GhostPlayer component.");
                        Destroy(ghost);
                    }
                }
            }
        }

        // 録画を開始
        if (playerRecorder != null)
        {
            playerRecorder.StartRecording();
        }
        else
        {
            Debug.LogError("Player prefab is missing GhostRecorder component.");
        }
    }

    // プレイヤーが死亡した時やリセットキーが押された時に呼ばれる
    public void ResetLoop()
    {
        Debug.Log("Resetting loop...");

        // 現在のプレイヤーの録画を停止し、記録を保存
        if (playerRecorder != null)
        {
            var newRecording = playerRecorder.StopRecording();
            if (newRecording != null && newRecording.Count > 0)
            {
                ghostRecordings.Add(newRecording);
                // 最大数を超えたら古いものから削除
                while (ghostRecordings.Count > maxGhostsPerStage)
                {
                    ghostRecordings.RemoveAt(0);
                }
            }
        }

        // 現在のシーンを再読み込みしてループをリスタート
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ステージクリア時に呼ばれる
    public void StageClear()
    {
        // 既にクリア処理が進行中なら何もしない
        if (isStageClearing) return;
        
        StartCoroutine(StageClearCoroutine());
    }

    private bool isStageClearing = false;
    private IEnumerator StageClearCoroutine()
    {
        isStageClearing = true;
        Debug.Log("Stage Clear!");

        // シーン内の全プレイヤーコントローラーを探し、"Player" タグを持つものを無効化する
        PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController controller in controllers)
        {
            if (controller.CompareTag("Player"))
            {
                controller.SetControllable(false);
                break; // プレイヤーは一人しかいないはずなので、見つけたらループを抜ける
            }
        }

        // ステージクリア・選択UIをまとめて表示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowStageSelectUI(true);
        }
        yield return null; // コルーチンを終了
    }

    // UIManagerから呼ばれ、指定されたステージをロードする
    public void LoadStage(string sceneName)
    {
        // UIを非表示にする
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowStageSelectUI(false);
        }

        // 状態をリセット
        Debug.Log($"Loading stage: {sceneName}");
        pulledLevers.Clear();
        ghostRecordings.Clear();
        isStageClearing = false; // 状態をリセット

        // シーンをロード
        SceneManager.LoadScene(sceneName);
    }

    // 次のステージのシーン名を取得する
    public string GetNextStageName()
    {
        switch (CurrentSceneName)
        {
            case "SampleScene":
                return "Stage2";
            case "Stage2":
                return "Stage3";
            // 他のステージが追加されたらここに追加
            default:
                Debug.LogWarning("No next stage defined for: " + CurrentSceneName);
                return "SampleScene"; // デフォルトは最初のステージに戻る
        }
    }

    // --- レバー管理用の新しいメソッド ---

    // LeverControllerのStart()から呼ばれ、シーンのレバーとして登録する
    public void RegisterLever(LeverController lever)
    {
        if (lever != null && !string.IsNullOrEmpty(lever.leverId) && !leversInScene.ContainsKey(lever.leverId))
        {
            leversInScene.Add(lever.leverId, lever);
        }
    }

    // LeverControllerのPullLever()から呼ばれ、状態を永続化する
    public void SetLeverAsPulled(string id)
    {
        if (!pulledLevers.Contains(id))
        {
            pulledLevers.Add(id);
        }
    }

    void OnDestroy()
    {
        // イベントの購読を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

#if UNITY_EDITOR
    private static string debug_startSceneName = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        // GameManagerがまだ存在しない場合（＝Bootシーン以外から再生した場合）
        if (Instance == null)
        {
            // 現在のシーン名を取得して、デバッグ開始シーンとして記憶
            string currentScene = SceneManager.GetActiveScene().name;
            // Bootシーン自身から再生した場合は、通常のフローに任せる
            if (currentScene != "Boot")
            {
                debug_startSceneName = currentScene;
            }

            // SystemManagersを生成
            var prefab = Resources.Load<GameObject>("SystemManagers");
            if (prefab != null)
            {
                Object.Instantiate(prefab);
            }
            else
            {
                Debug.LogError("SystemManagers prefab not found in Resources folder. Please create it.");
            }
        }
    }
#endif
}

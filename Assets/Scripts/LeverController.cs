using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LeverController : MonoBehaviour
{
    // このレバーが一意に識別されるためのID
    public string leverId;
    // このレバーが引かれたかどうかの状態
    public bool isPulled { get; private set; } = false;

    // レバーが引かれた時に実行されるイベント
    public UnityEvent onLeverPulled;

    private bool canInteract = false;
    private PlayerInput playerInput;
    private InputAction interactAction;

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーがインタラクト範囲に入った
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            // プレイヤーのInputSystemコンポーネントを取得
            playerInput = other.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                interactAction = playerInput.actions["Interact"];
            }
            // TODO: UIで「Eキーで操作」などを表示
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // プレイヤーがインタラクト範囲から出た
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            playerInput = null;
            interactAction = null;
            // TODO: UIを非表示に
        }
    }

    void Start()
    {
        // GameManagerに自身を登録させる
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterLever(this);
        }
    }

    void Update()
    {
        // インタラクト可能で、まだ引かれておらず、インタラクトキーが押された場合
        if (canInteract && !isPulled && interactAction != null && interactAction.triggered)
        {
            PullLever();
        }
    }

    public void PullLever()
    {
        if (isPulled) return; // 既に引かれていたら何もしない

        isPulled = true;
        Debug.Log($"Lever {leverId} has been pulled.");
        
        // GameManagerに状態の永続化を依頼
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetLeverAsPulled(leverId);
        }

        // 登録されたイベント（ブリッジ生成など）を実行
        onLeverPulled.Invoke();

        // 見た目を変更するなど（任意）
        // GetComponent<SpriteRenderer>().color = Color.red;
    }
    
    // GameManagerから状態を復元されるときに呼ばれる
    public void SetInitialState(bool pulled)
    {
        isPulled = pulled;
        if (isPulled)
        {
            onLeverPulled.Invoke();
        }
    }

    // Unityエディタで状態を確認するために、isPulledがtrueなら色を変える
    void OnDrawGizmos()
    {
        if(Application.isPlaying)
            Gizmos.color = isPulled ? Color.green : Color.yellow;
        else
            Gizmos.color = Color.yellow;
            
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

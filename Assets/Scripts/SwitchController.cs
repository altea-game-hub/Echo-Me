using UnityEngine;
using UnityEngine.Events;

public class SwitchController : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onSwitchPressed;
    public UnityEvent onSwitchExited;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ghost"))
        {
            Debug.Log("Switch was pressed.");
            // 登録されたイベント（壁を消すなど）を実行
            onSwitchPressed.Invoke();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ghost"))
        {
            Debug.Log("Player/Ghost exited switch.");
            // 登録されたイベント（壁を戻すなど）を実行
            onSwitchExited.Invoke();
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// 記録する入力の状態を定義する構造体
public struct InputState
{
    public float timeStamp;
    public Vector2 moveInput;       // 左右移動の入力 (-1 ~ 1)
    public bool jumpTriggered;      // ジャンプが押されたか
    public bool interactTriggered;  // インタラクトが押されたか
}

public class GhostRecorder : MonoBehaviour
{
    private List<InputState> recording;
    private bool isRecording = false;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction interactAction;

    // State tracking for reliable trigger detection in FixedUpdate
    private bool wasJumpPressedLastFrame = false;
    private bool wasInteractPressedLastFrame = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        interactAction = playerInput.actions["Interact"];
    }

    // ゲームマネージャから呼び出される
    public void StartRecording()
    {
        recording = new List<InputState>();
        isRecording = true;
        Debug.Log("Started recording.");
    }

    // ゲームマネージャから呼び出される
    public List<InputState> StopRecording()
    {
        isRecording = false;
        Debug.Log($"Stopped recording. Recorded {recording.Count} frames.");
        return new List<InputState>(recording); // コピーを渡す
    }

    // 物理演算と同期するため FixedUpdate を使用
    void FixedUpdate()
    {
        if (isRecording)
        {
            // --- Reliable trigger detection for recording ---
            bool isJumpPressed = jumpAction.IsPressed();
            bool jumpTriggeredThisFrame = isJumpPressed && !wasJumpPressedLastFrame;
            wasJumpPressedLastFrame = isJumpPressed;

            bool isInteractPressed = interactAction.IsPressed();
            bool interactTriggeredThisFrame = isInteractPressed && !wasInteractPressedLastFrame;
            wasInteractPressedLastFrame = isInteractPressed;

            recording.Add(new InputState
            {
                timeStamp = Time.time - GameManager.Instance.LoopStartTime,
                moveInput = moveAction.ReadValue<Vector2>(),
                jumpTriggered = jumpTriggeredThisFrame,
                interactTriggered = interactTriggeredThisFrame
            });
        }
        else
        {
            // リセットされていない状態を防ぐため、録画中でないときはリセットしておく
            wasJumpPressedLastFrame = false;
            wasInteractPressedLastFrame = false;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHandTrackingDebug : MonoBehaviour
{
    [SerializeField]
    private Transform leftHandTrackingRoot;

    private void Update()
    {
        if (leftHandTrackingRoot == null)
            return;

        // 使用新版 Unity Input System。
        // 按下 H 時輸出目前左手的世界座標與旋轉。
        if (Keyboard.current != null &&
            Keyboard.current.hKey.wasPressedThisFrame)
        {
            Debug.Log(
                $"[Left Hand Tracking] " +
                $"Position: {leftHandTrackingRoot.position}, " +
                $"Rotation: {leftHandTrackingRoot.eulerAngles}"
            );
        }
    }
}
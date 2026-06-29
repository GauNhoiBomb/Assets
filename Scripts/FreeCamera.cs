using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCamera : MonoBehaviour
{
    [Header("--- CÀI ĐẶT TỐC ĐỘ ---")]
    public float moveSpeed = 30f;
    public float heightSpeed = 20f;
    public float lookSpeed = 2f;
    public float keyRotateSpeed = 60f; // 💡 Tốc độ xoay camera bằng phím 1 và 3

    [Header("--- GIỚI HẠN GÓC NHÌN (PITCH) ---")]
    public float minPitch = 20f;  // Không cho ngửa lên nhìn trời
    public float maxPitch = 85f;  // Không cho cúi gập cổ quá sâu

    private float pitch;
    private float yaw;

    void OnEnable()
    {
        // Lấy góc nhìn hiện tại để chuyển đổi mượt mà
        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        // 1. DI CHUYỂN NGANG (WASD)
        Vector3 moveInput = new Vector3(
            (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
            0,
            (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
        );

        Vector3 forward = transform.forward; forward.y = 0; forward.Normalize();
        Vector3 right = transform.right; right.y = 0; right.Normalize();

        Vector3 moveDir = (forward * moveInput.z + right * moveInput.x).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // 2. NÂNG HẠ ĐỘ CAO (Q / E)
        float heightMove = (Keyboard.current.eKey.isPressed ? 1 : 0) - (Keyboard.current.qKey.isPressed ? 1 : 0);
        transform.position += Vector3.up * heightMove * heightSpeed * Time.deltaTime;

        // 💡 3. XOAY CAMERA BẰNG PHÍM 1 VÀ 3
        if (Keyboard.current.digit1Key.isPressed)
        {
            yaw -= keyRotateSpeed * Time.deltaTime; // Phím 1: Xoay sang Trái
        }
        if (Keyboard.current.digit3Key.isPressed)
        {
            yaw += keyRotateSpeed * Time.deltaTime; // Phím 3: Xoay sang Phải
        }

        // 💡 4. XOAY CAMERA BẰNG CHUỘT PHẢI (Giữ chuột phải để xoay màn hình)
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * lookSpeed * 0.1f;
            pitch -= mouseDelta.y * lookSpeed * 0.1f;
        }

        // Khóa góc cúi/ngửa và áp dụng góc xoay
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}
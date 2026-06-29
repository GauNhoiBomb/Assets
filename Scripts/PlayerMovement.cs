using UnityEngine;
using UnityEngine.InputSystem; // Gọi hệ thống điều khiển mới của Unity

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Thông số di chuyển")]
    public float walkSpeed = 2.5f; // Tốc độ đi bộ
    public float runSpeed = 6f;    // Tốc độ chạy (mặc định)
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Thông số Nhảy & Trọng lực")]
    public float jumpHeight = 1.5f; // Độ cao cú nhảy (Genshin thường khoảng 1.5 - 2)
    public float gravity = -15f;    // Trọng lực đầm hơn để rơi xuống nhanh
    private Vector3 velocity;

    [Header("Tham chiếu Camera")]
    public Transform cam; // Nơi kéo thả Main Camera vào
    public Animator anim; // KÉO NHÂN VẬT VÀO Ô NÀY TRONG INSPECTOR

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Khóa chuột vào giữa màn hình và ẩn đi để chơi game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


            // MỞ KHÓA CHUỘT cho game chiến thuật / thẻ bài
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
        
    }

    void Update()
    {
        // 1. Đọc phím WASD từ hệ thống New Input System
        float horizontal = 0f;
        float vertical = 0f;
        bool isWalking = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed) vertical -= 1f;
            if (Keyboard.current.dKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed) horizontal -= 1f;

            // Nhận diện phím Shift (Left Shift)
            if (Keyboard.current.leftShiftKey.isPressed) isWalking = true;
        }

        // Tạo vector hướng đi
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Xác định tốc độ hiện tại (Nếu giữ Shift thì Walk, không thì Run)
        float currentSpeed = isWalking ? walkSpeed : runSpeed;

        // Điều khiển Animation (0: Idle, 0.5: Walk, 1: Run)
        float animSpeedTarget = direction.magnitude * (isWalking ? 0.5f : 1f);
        if (anim != null)
        {
            // Làm mượt chuyển đổi animation
            anim.SetFloat("Speed", animSpeedTarget, 0.1f, Time.deltaTime);
        }

        // 2. Xử lý di chuyển theo hướng mắt Camera đang nhìn
        if (direction.magnitude >= 0.1f)
        {
            // Tính góc xoay
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Tính hướng tiến lên
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }

        // 3. Xử lý rớt xuống đất (Trọng lực)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Ép nhẹ xuống mặt đất để đi dốc không bị nảy
        }

        // 4. XỬ LÝ NHẢY (ẤN SPACE)
        // Kiểm tra nếu phím Space được bấm VÀ nhân vật đang đứng trên mặt đất
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && controller.isGrounded)
        {
            // Công thức vật lý tính vận tốc nhảy chuẩn xác
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Áp dụng trọng lực kéo xuống liên tục
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
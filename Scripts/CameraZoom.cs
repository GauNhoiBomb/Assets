using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem; // BẮT BUỘC THÊM: Gọi thư viện hệ thống điều khiển mới

[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class CameraZoom : MonoBehaviour
{
    [Header("Cài đặt Zoom")]
    // Lưu ý: Tôi đã giảm Zoom Speed xuống 0.05 vì hệ thống mới cuộn 1 nấc sẽ trả về số rất lớn (120)
    public float zoomSpeed = 0.05f;
    public float minDistance = 2f;
    public float maxDistance = 15f;

    private CinemachineOrbitalFollow orbitalFollow;

    void Start()
    {
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        // 1. Kiểm tra xem máy tính có đang cắm chuột không
        if (Mouse.current != null)
        {
            // 2. Đọc giá trị lăn chuột từ Input System mới
            float scroll = Mouse.current.scroll.y.ReadValue();

            if (scroll != 0f)
            {
                // 3. Thực hiện Zoom và khóa giới hạn (Clamp) y như cũ
                orbitalFollow.Radius -= scroll * zoomSpeed;
                orbitalFollow.Radius = Mathf.Clamp(orbitalFollow.Radius, minDistance, maxDistance);
            }
        }
    }
}
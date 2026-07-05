using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MouseControllerMoveButton : MonoBehaviour
{
    [Header("--- KẾT NỐI UI MANAGER ---")]
    public BattleUIManager_A uiManager;

    [Header("--- CÀI ĐẶT CON TRỎ CHUỘT (DI CHUYỂN) ---")]
    public LayerMask groundLayer;
    public Transform hoverCursor;

    [Header("--- HỆ THỐNG TARGET ENEMY ---")]
    public LayerMask enemyCharacterLayer;
    public Transform auraBox;
    [HideInInspector] public Transform lockedEnemy = null;

    [Header("--- TRẠNG THÁI BÀI (ĐỂ XỬ LÝ CHUỘT PHẢI) ---")]
    [Tooltip("Biến này bắt buộc phải được file Code Lá Bài cập nhật để kích hoạt tính năng hủy 2 bước!")]
    public bool isHoldingCard = false;

    [Header("--- KHAI BÁO TÊN LAYER LƯỚI ---")]
    public string playerLayerName = "PlayerGridLayerA001";

    [HideInInspector] public bool isTacticalMoveMode = false;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void OnDisable()
    {
        if (hoverCursor != null && hoverCursor.gameObject != null) hoverCursor.gameObject.SetActive(false);
        if (auraBox != null && auraBox.gameObject != null) auraBox.gameObject.SetActive(false);
        lockedEnemy = null;
    }

    // 💡 HÀM ĐỂ FILE LÁ BÀI CỦA BẠN GỌI SANG CỰC KỲ DỄ DÀNG
    public void SetHoldingCard(bool holding)
    {
        isHoldingCard = holding;
        Debug.Log($"[MOUSE CONTROLLER] Trạng thái cầm bài đổi thành: {holding}");
    }

    void Update()
    {
        if (Mouse.current == null || mainCam == null || uiManager == null) return;

        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool leftClick = Mouse.current.leftButton.wasPressedThisFrame;

        // 💡 SỬA LỖI HÀI HƯỚC: Đổi từ wasPressed sang wasReleased để đồng bộ nhả chuột với lá bài của bạn
        bool rightClickReleased = Mouse.current.rightButton.wasReleasedThisFrame;

        // =========================================================================
        // 1. XỬ LÝ CHUỘT PHẢI (TRÌNH TỰ ĐỒNG BỘ THEO PHA NHẢ CHUỘT)
        // =========================================================================
        if (rightClickReleased)
        {
            if (isHoldingCard)
            {
                // LẦN 1: Bạn đang chọn bài, nhả chuột phải ra thì CHỈ HỦY BÀI. 
                // Giữ an toàn tuyệt đối cho Target quái vật không bị mất.
                Debug.Log("🖱️ [CHUỘT PHẢI] Lần 1: Hủy chọn lá bài thành công. Bảo vệ Target quái an toàn!");

                // Lưu ý: Sau dòng này, code Lá bài của bạn phải tự động gọi SetHoldingCard(false) 
                // để lần bấm sau hệ thống biết đường hủy sang Target quái.
            }
            else if (lockedEnemy != null)
            {
                // LẦN 2: Khay bài sạch sẽ, nhả chuột phải ra mới HỦY TARGET QUÁI
                Debug.Log("❌ [CHUỘT PHẢI] Lần 2: Đã hủy khóa mục tiêu Quái vật.");
                lockedEnemy = null;
                uiManager.FocusEnemy(null);
            }
        }

        // =========================================================================
        // 2. PHÓNG TIA LASER (QUÁI VÀ ĐẤT CHẠY SONG SONG TẠI MỌI VCAM)
        // =========================================================================
        bool isHoveringEnemy = Physics.Raycast(ray, out RaycastHit enemyHit, 100f, enemyCharacterLayer);
        Transform hitEnemy = isHoveringEnemy ? enemyHit.transform : null;

        bool isHoveringGround = Physics.Raycast(ray, out RaycastHit groundHit, 100f, groundLayer);
        GameObject hitGroundObj = isHoveringGround ? groundHit.collider.gameObject : null;

        // =========================================================================
        // 3. XỬ LÝ CLICK CHUỘT TRÁI (ƯU TIÊN QUÁI > ĐẤT)
        // =========================================================================
        if (leftClick)
        {
            if (isHoveringEnemy)
            {
                lockedEnemy = hitEnemy;
                uiManager.FocusEnemy(lockedEnemy);
                Debug.Log($"🎯 [TARGET] Đã khóa mục tiêu: {hitEnemy.name}");
            }
            else if (isTacticalMoveMode && isHoveringGround)
            {
                GameObject targetCell = GetValidCell(hitGroundObj);
                if (targetCell != null && !uiManager.hasMovedThisPhase)
                {
                    uiManager.ExecuteMovementToCell(targetCell.transform.position);
                }
            }
        }

        // =========================================================================
        // 4. HIỂN THỊ AURA VÀ CON TRỎ (ĐỘC LẬP VỚI NHAU)
        // =========================================================================
        if (auraBox != null && auraBox.gameObject != null)
        {
            if (lockedEnemy != null)
            {
                if (!auraBox.gameObject.activeSelf) auraBox.gameObject.SetActive(true);
                auraBox.position = lockedEnemy.position + new Vector3(0, 0.05f, 0);
            }
            else if (isHoveringEnemy)
            {
                if (!auraBox.gameObject.activeSelf) auraBox.gameObject.SetActive(true);
                auraBox.position = hitEnemy.position + new Vector3(0, 0.05f, 0);
            }
            else
            {
                if (auraBox.gameObject.activeSelf) auraBox.gameObject.SetActive(false);
            }
        }

        if (isTacticalMoveMode && isHoveringGround)
        {
            GameObject targetCell = GetValidCell(hitGroundObj);
            if (targetCell != null)
            {
                if (hoverCursor != null && hoverCursor.gameObject != null)
                {
                    if (!hoverCursor.gameObject.activeSelf) hoverCursor.gameObject.SetActive(true);
                    hoverCursor.position = targetCell.transform.position + new Vector3(0, 0.05f, 0);
                }
            }
            else
            {
                if (hoverCursor != null && hoverCursor.gameObject != null) hoverCursor.gameObject.SetActive(false);
            }
        }
        else
        {
            if (hoverCursor != null && hoverCursor.gameObject != null) hoverCursor.gameObject.SetActive(false);
        }
    }

    private GameObject GetValidCell(GameObject hitObj)
    {
        BattleGridA001 grid = uiManager.battleGrid;
        if (grid != null && grid.validMoveCells != null)
        {
            if (grid.validMoveCells.Contains(hitObj)) return hitObj;
            if (hitObj.transform.parent != null && grid.validMoveCells.Contains(hitObj.transform.parent.gameObject)) return hitObj.transform.parent.gameObject;
        }
        return null;
    }
}
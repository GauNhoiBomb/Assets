using UnityEngine;
using UnityEngine.InputSystem;

public class MouseControllerMoveButton : MonoBehaviour
{
    [Header("--- KẾT NỐI UI MANAGER ---")]
    public BattleUIManager_A uiManager;

    [Header("--- CÀI ĐẶT CON TRỎ CHUỘT ---")]
    public LayerMask groundLayer;
    public Transform hoverCursor;

    [Header("--- HỆ THỐNG TARGET ENEMY ---")]
    public LayerMask enemyCharacterLayer;
    public Transform auraBox;
    [HideInInspector] public Transform lockedEnemy = null;

    [Header("--- KHAI BÁO TÊN LAYER LƯỚI ---")]
    public string playerLayerName = "PlayerGridLayerA001";

    [HideInInspector] public bool isTacticalMoveMode = false;

    private Camera mainCam;

    private Vector2 rightClickStartPos;
    private bool isRightDragging = false;

    void Start()
    {
        mainCam = Camera.main;
    }

    void OnDisable()
    {
        if (hoverCursor != null) hoverCursor.gameObject.SetActive(false);
        if (auraBox != null) auraBox.gameObject.SetActive(false);
        lockedEnemy = null;
    }

    void Update()
    {
        if (Mouse.current == null || mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool leftClick = Mouse.current.leftButton.wasPressedThisFrame;

        // ==========================================
        // XỬ LÝ CHUỘT PHẢI THÔNG MINH
        // ==========================================
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            rightClickStartPos = Mouse.current.position.ReadValue();
            isRightDragging = false;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            if (Vector2.Distance(Mouse.current.position.ReadValue(), rightClickStartPos) > 5f)
            {
                isRightDragging = true;
            }
        }

        bool isRightClickRelease = Mouse.current.rightButton.wasReleasedThisFrame && !isRightDragging;

        // 💡 KHÓA BẢO VỆ TỐI THƯỢNG: Đang cầm bài, hoặc VỪA MỚI vứt bài trong chớp mắt?
        bool isHoldingOrJustDroppedCard = false;
        if (uiManager != null && uiManager.battleManager != null && uiManager.battleManager.handManager != null)
        {
            HandManager handMgr = uiManager.battleManager.handManager;
            if (handMgr.currentSelectedCard != null || handMgr.dropCardFrame == Time.frameCount)
            {
                isHoldingOrJustDroppedCard = true;
            }
        }

        // ==========================================
        // 1. QUÉT QUÁI VẬT
        // ==========================================
        bool isHoveringEnemy = Physics.Raycast(ray, out RaycastHit enemyHit, 1000f, enemyCharacterLayer);
        Transform targetEnemy = lockedEnemy != null ? lockedEnemy : (isHoveringEnemy ? enemyHit.transform : null);

        if (targetEnemy != null)
        {
            if (auraBox != null)
            {
                if (!auraBox.gameObject.activeSelf) auraBox.gameObject.SetActive(true);
                auraBox.position = targetEnemy.position + new Vector3(0, 0.05f, 0);
            }

            if (leftClick && isHoveringEnemy)
            {
                lockedEnemy = enemyHit.transform;
                if (uiManager != null) uiManager.FocusEnemy(lockedEnemy);
            }
        }
        else
        {
            if (auraBox != null && auraBox.gameObject.activeSelf) auraBox.gameObject.SetActive(false);
        }

        // 💡 ĐÃ SỬA: CHỈ HỦY TARGET NẾU KHÔNG ĐỘNG CHẠM GÌ TỚI LÁ BÀI
        if (isRightClickRelease && !isTacticalMoveMode && lockedEnemy != null && !isHoldingOrJustDroppedCard)
        {
            lockedEnemy = null;
            if (uiManager != null) uiManager.FocusEnemy(null);
        }

        // ==========================================
        // 2. CHẾ ĐỘ DI CHUYỂN (TACTICAL MOVE)
        // ==========================================
        if (isTacticalMoveMode)
        {
            if (isRightClickRelease && uiManager != null && !isHoldingOrJustDroppedCard)
            {
                if (uiManager.hasMovedThisPhase)
                {
                    uiManager.UndoMovement();
                }
            }

            if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundLayer))
            {
                if (groundHit.transform.gameObject.layer == LayerMask.NameToLayer(playerLayerName))
                {
                    Transform cellRoot = groundHit.transform.name == "Quad" ? groundHit.transform.parent : groundHit.transform;

                    if (uiManager.battleGrid != null && uiManager.battleGrid.validMoveCells.Contains(cellRoot.gameObject))
                    {
                        if (hoverCursor != null)
                        {
                            if (!hoverCursor.gameObject.activeSelf) hoverCursor.gameObject.SetActive(true);
                            hoverCursor.position = cellRoot.position + new Vector3(0, 0.05f, 0);
                        }

                        if (leftClick && uiManager != null && !uiManager.hasMovedThisPhase)
                        {
                            uiManager.ExecuteMovementToCell(cellRoot.position);
                        }
                    }
                    else
                    {
                        if (hoverCursor != null && hoverCursor.gameObject.activeSelf) hoverCursor.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (hoverCursor != null && hoverCursor.gameObject.activeSelf) hoverCursor.gameObject.SetActive(false);
                }
            }
            else
            {
                if (hoverCursor != null && hoverCursor.gameObject.activeSelf) hoverCursor.gameObject.SetActive(false);
            }
        }
        else
        {
            if (hoverCursor != null && hoverCursor.gameObject.activeSelf) hoverCursor.gameObject.SetActive(false);
        }
    }
}
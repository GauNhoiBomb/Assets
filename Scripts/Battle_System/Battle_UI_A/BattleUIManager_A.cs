using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Unity.Cinemachine;

public class BattleUIManager_A : MonoBehaviour
{
    [Header("--- KẾT NỐI CAMERA CINEMACHINE ---")]
    public CinemachineCamera vcamDefault;
    public CinemachineCamera vcamTactical;
    public FreeCamera freeCameraScript;

    [Header("--- CÀI ĐẶT NHÂN VẬT ---")]
    public Transform activeCharacter;
    public bool isLargeCharacter = false;

    [Header("--- KẾT NỐI KHÁC ---")]
    public BattleManager battleManager;
    public PlayerStats playerStats;
    public MouseControllerMoveButton gridMouseController;
    public BattleGridA001 battleGrid;

    [Header("--- QUẢN LÝ PANEL UI ---")]
    public GameObject normalBottomUIPanel;
    public GameObject tacticalButtonsPanel;
    public TextMeshProUGUI waitEndText;
    public GameObject runConfirmPanel;

    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public bool hasMovedThisPhase = false;
    public UnityEvent onAPChanged;

    private bool hasPlayedCardThisTurn = false;
    private float defaultTacticalYRotation;

    [Header("--- 💡 CÀI ĐẶT GÓC CAMERA MẶC ĐỊNH ---")]
    public Vector3 defaultCameraOffset = new Vector3(-5.5f, 3f, 3f);

    [Header("--- 💡 HỆ THỐNG CAMERA QUA VAI (OTS) ---")]
    public float cameraMoveSpeed = 5f;
    public float framingOffsetX = 4.5f;
    public float enemyScreenPanX = -4f;

    [HideInInspector] public float baseCameraDistance = 3f;
    [HideInInspector] public float cameraHeightOffset = 1.7f;

    [Header("--- 🚀 THUẬT TOÁN BIỂU ĐỒ (KHOẢNG CÁCH TUYỆT ĐỐI) ---")]
    public bool useAutoCameraTuning = true;

    [Tooltip("Trục Ngang (Time) LÀ KHOẢNG CÁCH THỰC. Trục Dọc (Value) LÀ BASE DISTANCE")]
    public AnimationCurve distanceCurve;

    [Tooltip("Trục Ngang (Time) LÀ KHOẢNG CÁCH THỰC. Trục Dọc (Value) LÀ CAMERA HEIGHT")]
    public AnimationCurve heightCurve;

    private Transform currentEnemyTarget = null;
    private Quaternion defaultDefaultRotation;

    void Start()
    {
        if (runConfirmPanel != null) runConfirmPanel.SetActive(false);
        if (battleGrid != null) battleGrid.SetGridVisibility(false, false);

        if (normalBottomUIPanel != null) normalBottomUIPanel.SetActive(true);
        if (tacticalButtonsPanel != null) tacticalButtonsPanel.SetActive(false);

        // 💡 BẬT CẢ 2 CAMERA VÀ THIẾT LẬP PRIORITY ĐỂ BLENDING MƯỢT MÀ
        if (vcamDefault != null)
        {
            vcamDefault.gameObject.SetActive(true);
            vcamDefault.Priority = 20; // Đặt quyền ưu tiên cao hơn lúc khởi đầu
            vcamDefault.Target.TrackingTarget = null;
            vcamDefault.Target.LookAtTarget = null;
            defaultDefaultRotation = vcamDefault.transform.rotation;
        }

        if (vcamTactical != null)
        {
            vcamTactical.gameObject.SetActive(true);
            vcamTactical.Priority = 10; // Đặt quyền ưu tiên thấp hơn để chạy ngầm
            defaultTacticalYRotation = vcamTactical.transform.eulerAngles.y;
        }

        if (freeCameraScript != null) freeCameraScript.enabled = false;
        ResetTurnState();
    }

    public void FocusEnemy(Transform enemyTarget)
    {
        currentEnemyTarget = enemyTarget;
    }

    // 💡 HÀM MỚI: Gọi để gán Focus và tự động reset Camera về Default
    public void SetActiveCharacterFocus(Transform newCharacter)
    {
        activeCharacter = newCharacter;
        // Trả camera về góc Default nếu trước đó người chơi đang ngắm Grid chưa thoát ra
        if (vcamTactical != null && vcamTactical.Priority > 10)
        {
            CancelMoveMode();
        }
    }

    void Update()
    {
        // Kiểm tra xem camera Default có đang chiếm quyền không (Priority lớn hơn Tactical)
        if (vcamDefault != null && vcamDefault.Priority > vcamTactical.Priority)
        {
            if (currentEnemyTarget != null && activeCharacter != null)
            {
                // ==========================================================
                // 🚀 ĐỌC BIỂU ĐỒ BẰNG KHOẢNG CÁCH THỰC TẾ TRONG UNITY
                // ==========================================================
                if (useAutoCameraTuning)
                {
                    float actualZDist = Mathf.Abs(currentEnemyTarget.position.z - activeCharacter.position.z);
                    baseCameraDistance = distanceCurve.Evaluate(actualZDist);
                    cameraHeightOffset = heightCurve.Evaluate(actualZDist);
                }

                // ==========================================================
                // CÁC HÀM XỬ LÝ GÓC NHÌN VÀ VỊ TRÍ
                // ==========================================================
                float enemyHeight = 1f;
                Collider enemyCol = currentEnemyTarget.GetComponent<Collider>();
                if (enemyCol != null) enemyHeight = enemyCol.bounds.size.y;

                Vector3 enemyFocusPoint = currentEnemyTarget.position + new Vector3(0, enemyHeight * 0.7f, 0);
                Vector3 playerFocusPoint = activeCharacter.position + new Vector3(0, 1f, 0);

                Vector3 baseLookDir = (enemyFocusPoint - playerFocusPoint).normalized;
                if (baseLookDir == Vector3.zero) baseLookDir = Vector3.forward;
                Quaternion baseRot = Quaternion.LookRotation(baseLookDir);

                Vector3 localOffset = new Vector3(framingOffsetX, cameraHeightOffset, -baseCameraDistance);
                Vector3 targetCameraPosition = playerFocusPoint + (baseRot * localOffset);

                Vector3 virtualLookTarget = enemyFocusPoint + (baseRot * Vector3.right * enemyScreenPanX);
                Quaternion targetRotation = Quaternion.LookRotation(virtualLookTarget - targetCameraPosition);

                vcamDefault.transform.position = Vector3.Lerp(vcamDefault.transform.position, targetCameraPosition, Time.deltaTime * cameraMoveSpeed);
                vcamDefault.transform.rotation = Quaternion.Slerp(vcamDefault.transform.rotation, targetRotation, Time.deltaTime * cameraMoveSpeed);
            }
            else if (activeCharacter != null)
            {
                Vector3 targetDefaultPosition = activeCharacter.position + defaultCameraOffset;

                vcamDefault.transform.position = Vector3.Lerp(vcamDefault.transform.position, targetDefaultPosition, Time.deltaTime * cameraMoveSpeed);
                vcamDefault.transform.rotation = Quaternion.Slerp(vcamDefault.transform.rotation, defaultDefaultRotation, Time.deltaTime * cameraMoveSpeed);
            }
        }
    }

    public void OnMoveClicked()
    {
        if (activeCharacter != null) originalPosition = activeCharacter.position;
        hasMovedThisPhase = false;

        if (normalBottomUIPanel != null) normalBottomUIPanel.SetActive(false);
        if (tacticalButtonsPanel != null) tacticalButtonsPanel.SetActive(true);

        if (battleGrid != null) battleGrid.SetGridVisibility(true, false);
        if (gridMouseController != null) gridMouseController.isTacticalMoveMode = true;

        // 💡 CHUYỂN GÓC NHÌN BẰNG PRIORITY MƯỢT MÀ
        if (vcamDefault != null) vcamDefault.Priority = 10;
        if (vcamTactical != null) vcamTactical.Priority = 20;

        OnFocusCamButtonClicked();
    }

    public void OnFreeCamButtonClicked()
    {
        if (vcamTactical != null)
        {
            vcamTactical.Target.TrackingTarget = null;
            vcamTactical.Target.LookAtTarget = null;
            if (freeCameraScript != null) freeCameraScript.enabled = true;
        }
    }

    public void OnFocusCamButtonClicked()
    {
        if (vcamTactical != null && activeCharacter != null)
        {
            if (freeCameraScript != null) freeCameraScript.enabled = false;

            vcamTactical.Target.TrackingTarget = activeCharacter;
            vcamTactical.Target.LookAtTarget = null;

            vcamTactical.transform.rotation = Quaternion.Euler(45f, defaultTacticalYRotation, 0f);
        }
    }

    public void ExecuteMovementToCell(Vector3 targetPos)
    {
        if (playerStats != null && playerStats.currentAP >= 3)
        {
            playerStats.SpendAP(3);

            if (activeCharacter != null)
            {
                CharacterController cc = activeCharacter.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                activeCharacter.position = new Vector3(targetPos.x, activeCharacter.position.y, targetPos.z);
                if (cc != null) cc.enabled = true;
            }
            hasMovedThisPhase = true;
            if (battleGrid != null) battleGrid.RefreshGridTacticalColors();
            onAPChanged?.Invoke();
        }
    }

    public void UndoMovement()
    {
        if (activeCharacter != null)
        {
            CharacterController cc = activeCharacter.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            activeCharacter.position = originalPosition;
            if (cc != null) cc.enabled = true;
        }
        if (playerStats != null) { playerStats.currentAP += 3; playerStats.UpdateUI(); }
        hasMovedThisPhase = false;
        if (battleGrid != null) battleGrid.RefreshGridTacticalColors();
        onAPChanged?.Invoke();
    }

    public void OnBackButtonClicked()
    {
        CancelMoveMode();
    }

    public void CancelMoveMode()
    {
        if (battleGrid != null) battleGrid.SetGridVisibility(false, false);
        if (gridMouseController != null) gridMouseController.isTacticalMoveMode = false;

        if (freeCameraScript != null) freeCameraScript.enabled = false;

        // 💡 TRẢ LẠI GÓC NHÌN DEFAULT BẰNG PRIORITY
        if (vcamTactical != null) vcamTactical.Priority = 10;
        if (vcamDefault != null)
        {
            vcamDefault.Priority = 20;
            vcamDefault.Target.TrackingTarget = null;
            vcamDefault.Target.LookAtTarget = null;
        }

        if (tacticalButtonsPanel != null) tacticalButtonsPanel.SetActive(false);
        if (normalBottomUIPanel != null) normalBottomUIPanel.SetActive(true);

        onAPChanged?.Invoke();
    }

    public void ResetTurnState() { hasPlayedCardThisTurn = false; if (waitEndText != null) waitEndText.text = "WAIT"; }
    public void MarkCardPlayed() { hasPlayedCardThisTurn = true; if (waitEndText != null) waitEndText.text = "END"; }
    public void SetWaitPhaseState() { hasPlayedCardThisTurn = true; if (waitEndText != null) waitEndText.text = "END"; }

    public void OnWaitOrEndClicked()
    {
        if (!hasPlayedCardThisTurn) { if (battleManager != null) battleManager.OnWaitButtonClicked(); }
        else { if (battleManager != null) battleManager.PlayerEndTurn(); ResetTurnState(); }
    }

    public void OnRunClicked() { if (runConfirmPanel != null) runConfirmPanel.SetActive(true); }
    public void OnConfirmRunClicked() { Debug.Log("BỎ CHẠY THÀNH CÔNG!"); }
    public void OnCancelRunClicked() { if (runConfirmPanel != null) runConfirmPanel.SetActive(false); }
}
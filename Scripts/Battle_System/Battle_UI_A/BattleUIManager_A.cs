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

    public bool useAutoCameraTuning = true;
    public AnimationCurve distanceCurve;
    public AnimationCurve heightCurve;

    private Transform currentEnemyTarget = null;
    private Quaternion defaultDefaultRotation;

    void Start()
    {
        if (runConfirmPanel != null) runConfirmPanel.SetActive(false);
        if (battleGrid != null) battleGrid.SetGridVisibility(false, false);

        if (normalBottomUIPanel != null) normalBottomUIPanel.SetActive(true);
        if (tacticalButtonsPanel != null) tacticalButtonsPanel.SetActive(false);

        if (vcamDefault != null)
        {
            vcamDefault.gameObject.SetActive(true);
            vcamDefault.Priority = 20;
            vcamDefault.Target.TrackingTarget = null;
            vcamDefault.Target.LookAtTarget = null;
            defaultDefaultRotation = vcamDefault.transform.rotation;
        }

        if (vcamTactical != null)
        {
            vcamTactical.gameObject.SetActive(true);
            vcamTactical.Priority = 10;
            defaultTacticalYRotation = vcamTactical.transform.eulerAngles.y;
        }

        if (freeCameraScript != null) freeCameraScript.enabled = false;
        ResetTurnState();
    }

    public void FocusEnemy(Transform enemyTarget)
    {
        currentEnemyTarget = enemyTarget;

        if (currentEnemyTarget != null && activeCharacter != null)
        {
            Vector3 lookDir = currentEnemyTarget.position - activeCharacter.position;
            lookDir.y = 0;

            if (lookDir != Vector3.zero)
            {
                activeCharacter.rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }

    public void SetActiveCharacterFocus(Transform newCharacter)
    {
        activeCharacter = newCharacter;
        if (vcamTactical != null && vcamTactical.Priority > 10)
        {
            CancelMoveMode();
        }
    }

    void Update()
    {
        if (vcamDefault != null && vcamDefault.Priority > vcamTactical.Priority)
        {
            if (currentEnemyTarget != null && activeCharacter != null)
            {
                if (useAutoCameraTuning)
                {
                    float actualZDist = Mathf.Abs(currentEnemyTarget.position.z - activeCharacter.position.z);
                    baseCameraDistance = distanceCurve.Evaluate(actualZDist);
                    cameraHeightOffset = heightCurve.Evaluate(actualZDist);
                }

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
        if (battleManager == null || battleManager.currentActiveUnit == null) return;
        BattleUnit activeUnit = battleManager.currentActiveUnit;

        if (activeUnit.currentAP >= 3)
        {
            activeUnit.currentAP -= 3;
            if (playerStats != null) playerStats.SpendAP(3);

            hasMovedThisPhase = true;
            RefreshWaitEndButton();

            if (battleGrid != null)
            {
                if (battleGrid.activePlayerUnit == null) battleGrid.activePlayerUnit = activeCharacter;
                battleGrid.RefreshGridTacticalColors();
            }

            try { if (onAPChanged != null) onAPChanged.Invoke(); } catch { }

            if (activeCharacter != null)
            {
                if (battleManager != null && battleManager.gameObject.activeInHierarchy)
                {
                    battleManager.StartCoroutine(SmoothMoveToCell(activeCharacter, targetPos));
                }
                else
                {
                    gameObject.SetActive(true);
                    StartCoroutine(SmoothMoveToCell(activeCharacter, targetPos));
                }
            }
        }
    }

    public System.Collections.IEnumerator SmoothMoveToCell(Transform unitTransform, Vector3 targetPos)
    {
        if (unitTransform == null) yield break;

        CharacterController cc = unitTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Animator anim = unitTransform.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetFloat("Speed", 1f);

        // Xoay mặt nhìn về hướng ô đất đích để chạy tới
        Vector3 moveDir = (targetPos - unitTransform.position).normalized;
        Vector3 lookDir = new Vector3(moveDir.x, 0, moveDir.z);
        if (lookDir != Vector3.zero) unitTransform.rotation = Quaternion.LookRotation(lookDir);

        float moveSpeed = 8f;

        while (Vector3.Distance(new Vector3(unitTransform.position.x, 0, unitTransform.position.z),
                                new Vector3(targetPos.x, 0, targetPos.z)) > 0.1f)
        {
            unitTransform.position = Vector3.MoveTowards(unitTransform.position,
                new Vector3(targetPos.x, unitTransform.position.y, targetPos.z), moveSpeed * Time.deltaTime);
            yield return null;
        }

        unitTransform.position = new Vector3(targetPos.x, unitTransform.position.y, targetPos.z);
        if (cc != null) cc.enabled = true;

        if (anim != null) anim.SetFloat("Speed", 0f);
    }

    public void UndoMovement()
    {
        if (battleManager == null || battleManager.currentActiveUnit == null) return;
        BattleUnit activeUnit = battleManager.currentActiveUnit;

        if (activeCharacter != null)
        {
            CharacterController cc = activeCharacter.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            activeCharacter.position = originalPosition;
            if (cc != null) cc.enabled = true;
        }

        activeUnit.currentAP += 3;
        if (playerStats != null)
        {
            playerStats.currentAP += 3;
            playerStats.UpdateUI();
        }

        hasMovedThisPhase = false;
        RefreshWaitEndButton();

        if (battleGrid != null) battleGrid.RefreshGridTacticalColors();

        try { if (onAPChanged != null) onAPChanged.Invoke(); } catch { }
    }

    public void OnBackButtonClicked() { CancelMoveMode(); }

    public void CancelMoveMode()
    {
        if (battleGrid != null) battleGrid.SetGridVisibility(false, false);
        if (gridMouseController != null) gridMouseController.isTacticalMoveMode = false;

        if (freeCameraScript != null) freeCameraScript.enabled = false;

        if (vcamTactical != null) vcamTactical.Priority = 10;
        if (vcamDefault != null)
        {
            vcamDefault.Priority = 20;
            vcamDefault.Target.TrackingTarget = null;
            vcamDefault.Target.LookAtTarget = null;
        }

        if (tacticalButtonsPanel != null) tacticalButtonsPanel.SetActive(false);
        if (normalBottomUIPanel != null) normalBottomUIPanel.SetActive(true);

        // =========================================================================
        // 💡 ÁP DỤNG ĐỀ XUẤT CỦA BẠN: Khi bấm nút Back thoát lưới, 
        // nếu hệ thống đang ghi nhận có Quái vật bị khóa Target, ép nhân vật xoay mặt nhìn thẳng vào nó lập tức!
        // =========================================================================
        if (currentEnemyTarget != null && activeCharacter != null)
        {
            Vector3 lookDir = currentEnemyTarget.position - activeCharacter.position;
            lookDir.y = 0; // Giữ cân bằng trục Y chống ngửa người
            if (lookDir != Vector3.zero)
            {
                activeCharacter.rotation = Quaternion.LookRotation(lookDir);
                Debug.Log($"🔄 [BACK] Đã tự động xoay nhân vật nhìn thẳng về phía mục tiêu: {currentEnemyTarget.name}");
            }
        }

        try { if (onAPChanged != null) onAPChanged.Invoke(); } catch { }
    }

    public void RefreshWaitEndButton()
    {
        if (waitEndText != null)
        {
            if (hasPlayedCardThisTurn || hasMovedThisPhase)
                waitEndText.text = "END";
            else
                waitEndText.text = "WAIT";
        }
    }

    public void ResetTurnState()
    {
        hasPlayedCardThisTurn = false;
        hasMovedThisPhase = false;
        RefreshWaitEndButton();
    }

    public void MarkCardPlayed()
    {
        hasPlayedCardThisTurn = true;
        RefreshWaitEndButton();
    }

    // 💡 ĐÃ BỔ SUNG LẠI HÀM NÀY: Giúp sửa triệt để lỗi biên dịch đỏ lòm trên BattleManager
    public void SetWaitPhaseState()
    {
        hasPlayedCardThisTurn = true;
        RefreshWaitEndButton();
    }

    public void OnWaitOrEndClicked()
    {
        if (!hasPlayedCardThisTurn && !hasMovedThisPhase)
        {
            if (battleManager != null) battleManager.OnWaitButtonClicked();
        }
        else
        {
            if (battleManager != null) battleManager.PlayerEndTurn();
            ResetTurnState();
        }
    }

    public void OnRunClicked() { if (runConfirmPanel != null) runConfirmPanel.SetActive(true); }
    public void OnConfirmRunClicked() { Debug.Log("BỎ CHẠY THÀNH CÔNG!"); }
    public void OnCancelRunClicked() { if (runConfirmPanel != null) runConfirmPanel.SetActive(false); }
}
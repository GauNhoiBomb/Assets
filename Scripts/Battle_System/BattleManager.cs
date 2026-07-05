using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ==========================================
// 💡 ĐÃ KHÔI PHỤC: ĐỊNH NGHĨA TRẠNG THÁI TRẬN ĐẤU
// note: Enum này bắt buộc phải nằm ở đây để biến "state" bên dưới có dữ liệu hoạt động!
// ==========================================
public enum BattleState { Start, DrawCards, NormalTurnPhase, WaitTurnPhase, Won, Lost }

public class BattleManager : MonoBehaviour
{
    [Header("--- THÔNG TIN TRẬN ĐẤU ---")]
    public BattleState state;

    [Header("--- KẾT NỐI UI CHÍNH VÀ CAMERA ---")]
    public HandManager handManager;
    public TurnTimelineManager timelineManager;
    public AnomalyAndTurnStatusPanelUI statusUI;

    // 💡 KẾT NỐI: Cầu nối quản lý UI & Camera trận đấu (Cinemachine Priority)
    public BattleUIManager_A uiManager;

    public int currentRoundCount = 1;

    [Header("--- KẾT NỐI NGƯỜI QUẢN LÝ KHÁC ---")]
    public AnomalyManager anomalyManager;
    public BattleAIManager aiManager;

    [Header("--- DANH SÁCH THAM CHIẾN (DATA GỐC) ---")]
    public List<CharacterStats> playerTeamData;
    public List<CharacterStats> enemyTeamData;

    // 💡 ĐỒNG BỘ: Nơi kéo thả Model 3D thực tế ngoài Scene vào trận đấu
    // note: Kéo từ Hierarchy vào đúng thứ tự tương ứng với Data gốc ở trên
    [Header("--- DIỄN VIÊN 3D (KÉO TỪ HIERARCHY VÀO ĐÂY) ---")]
    public List<Transform> playerModels;
    public List<Transform> enemyModels;

    [Header("--- LUẬT BỐC BÀI ---")]
    public int baseItemDrawAmount = 3;
    public int baseSkillDrawAmount = 3;

    [Header("--- DANH SÁCH THỰC THỂ SỐNG ---")]
    public List<BattleUnit> allUnitsInBattle = new List<BattleUnit>();

    [Header("--- HỆ THỐNG LƯỢT ĐI (HOMM3 STYLE) ---")]
    public List<BattleUnit> normalTurnQueue = new List<BattleUnit>();
    public List<BattleUnit> waitTurnQueue = new List<BattleUnit>();
    public List<BattleUnit> nextRoundQueue = new List<BattleUnit>();
    public BattleUnit currentActiveUnit;

    void Start()
    {
        if (timelineManager == null) timelineManager = FindAnyObjectByType<TurnTimelineManager>();
        state = BattleState.Start;
        SetupBattle();
    }

    void SetupBattle()
    {
        Debug.Log("TRẬN ĐẤU BẮT ĐẦU!");

        // Gắn xương thịt (Model 3D) và Nạp bài cá nhân cho phe Player
        for (int i = 0; i < playerTeamData.Count; i++)
        {
            BattleUnit unit = new BattleUnit(playerTeamData[i]);
            if (i < playerModels.Count) unit.unitTransform = playerModels[i];
            InitializePersonalDeck(unit);
            allUnitsInBattle.Add(unit);
        }

        // Gắn xương thịt (Model 3D) và Nạp bài cá nhân cho phe Địch / NPC
        for (int i = 0; i < enemyTeamData.Count; i++)
        {
            BattleUnit unit = new BattleUnit(enemyTeamData[i]);
            if (i < enemyModels.Count) unit.unitTransform = enemyModels[i];
            InitializePersonalDeck(unit);
            allUnitsInBattle.Add(unit);
        }

        if (anomalyManager != null) anomalyManager.InitializeAnomaliesOnStart();
        StartNewRound();
    }

    // 💡 KHỞI TẠO BỘ BÀI RIÊNG CỦA TỪNG NHÂN VẬT
    void InitializePersonalDeck(BattleUnit unit)
    {
        if (unit.baseData.personalDeck == null) return;

        // Nạp và kiểm tra Item chống ô rỗng (None) ngoài Inspector
        foreach (ItemCardData itemData in unit.baseData.personalDeck.activeItemCards)
        {
            if (itemData != null) unit.itemDrawPile.Add(new ItemCardInstance(itemData, itemData.maxDurability));
        }

        // Nạp và kiểm tra Skill chống ô rỗng (None) ngoài Inspector
        foreach (SkillCardData skillData in unit.baseData.personalDeck.activeSkillCards)
        {
            if (skillData != null) unit.skillDrawPile.Add(new SkillCardInstance(skillData));
        }

        ShuffleList(unit.itemDrawPile);
        ShuffleList(unit.skillDrawPile);
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // ==========================================
    // KHỞI TẠO ROUND MỚI (CHUYỂN KHAY B THÀNH KHAY A)
    // ==========================================
    void StartNewRound()
    {
        if (state != BattleState.Start) currentRoundCount++;
        if (statusUI != null) statusUI.UpdateTurnCounter(currentRoundCount);

        if (anomalyManager != null) anomalyManager.ProcessAnomalies(AnomalyTriggerTime.TurnStart);

        if (nextRoundQueue.Count == 0 && normalTurnQueue.Count == 0 && waitTurnQueue.Count == 0)
            normalTurnQueue = allUnitsInBattle.ToList();
        else
        {
            normalTurnQueue = nextRoundQueue.ToList();
            nextRoundQueue.Clear();
        }

        normalTurnQueue = normalTurnQueue.OrderByDescending(unit => unit.currentAGI).ToList();
        waitTurnQueue.Clear();

        // 💡 BƯỚC 3: Hồi lại 3 AP mặc định cho TẤT CẢ mọi người ở đầu Hiệp mới
        foreach (var unit in allUnitsInBattle)
        {
            unit.currentAP = 3;
        }

        state = BattleState.DrawCards;
        DrawPhase();
    }

    // 💡 BƯỚC 2: Rút bài Cách B (Tất cả mọi người đồng loạt bốc đủ bài vào đầu hiệp)
    void DrawPhase()
    {
        foreach (BattleUnit unit in allUnitsInBattle)
        {
            // Vứt hết bài cũ còn sót trên tay vào mộ (Discard Pile) trước khi bốc mới
            unit.itemDiscardPile.AddRange(unit.itemHand); unit.itemHand.Clear();
            unit.skillDiscardPile.AddRange(unit.skillHand); unit.skillHand.Clear();

            // Tiến hành rút bài từ Draw Pile cá nhân
            DrawCardsForUnit(unit, baseItemDrawAmount, baseSkillDrawAmount);
        }

        state = BattleState.NormalTurnPhase;
        NextTurn();
    }

    void DrawCardsForUnit(BattleUnit unit, int itemAmount, int skillAmount)
    {
        // Rút Item Card
        for (int i = 0; i < itemAmount; i++)
        {
            if (unit.itemDrawPile.Count == 0)
            {
                if (unit.itemDiscardPile.Count == 0) break;
                unit.itemDrawPile.AddRange(unit.itemDiscardPile); unit.itemDiscardPile.Clear(); ShuffleList(unit.itemDrawPile);
            }
            if (unit.itemDrawPile.Count > 0) { unit.itemHand.Add(unit.itemDrawPile[0]); unit.itemDrawPile.RemoveAt(0); }
        }

        // Rút Skill Card
        for (int i = 0; i < skillAmount; i++)
        {
            if (unit.skillDrawPile.Count == 0)
            {
                if (unit.skillDiscardPile.Count == 0) break;
                unit.skillDrawPile.AddRange(unit.skillDiscardPile); unit.skillDiscardPile.Clear(); ShuffleList(unit.skillDrawPile);
            }
            if (unit.skillDrawPile.Count > 0) { unit.skillHand.Add(unit.skillDrawPile[0]); unit.skillDrawPile.RemoveAt(0); }
        }
    }

    void NextTurn()
    {
        if (normalTurnQueue.Count > 0)
        {
            currentActiveUnit = normalTurnQueue[0];
            normalTurnQueue.RemoveAt(0);
        }
        else if (waitTurnQueue.Count > 0)
        {
            state = BattleState.WaitTurnPhase;
            waitTurnQueue = waitTurnQueue.OrderBy(unit => unit.currentAGI).ToList();
            currentActiveUnit = waitTurnQueue[0];
            waitTurnQueue.RemoveAt(0);
            if (handManager != null && handManager.uiManager != null) handManager.uiManager.SetWaitPhaseState();
        }
        else
        {
            Debug.Log("HẾT TURN 1! BẮT ĐẦU TURN 2...");
            StartNewRound();
            return;
        }

        if (timelineManager != null) timelineManager.RebuildTimeline(currentActiveUnit, normalTurnQueue, waitTurnQueue, nextRoundQueue);
        HandleUnitTurnStart();
    }

    void HandleUnitTurnStart()
    {
        if (currentActiveUnit == null) return;

        // 💡 ĐỒNG BỘ: Đẩy dữ liệu vật lý (Transform) sang cho Camera và Lưới Grid
        if (uiManager != null && currentActiveUnit.unitTransform != null)
        {
            // 1. Ra lệnh cho Camera focus cận cảnh vào nhân vật vừa tới lượt
            uiManager.SetActiveCharacterFocus(currentActiveUnit.unitTransform);

            // 2. Ra lệnh cho Lưới Grid chuyển tâm vẽ ô xanh về dưới chân nhân vật vừa tới lượt
            if (uiManager.battleGrid != null)
            {
                uiManager.battleGrid.activePlayerUnit = currentActiveUnit.unitTransform;

                // note: Chỉ vẽ lưới xanh nếu là nhân vật phe Ta (Player). Quái/NPC đi thì ẩn lưới đi.
                if (currentActiveUnit.role == CharacterRole.Player)
                {
                    uiManager.battleGrid.RefreshGridTacticalColors();
                }
                else
                {
                    uiManager.battleGrid.validMoveCells.Clear();
                    uiManager.battleGrid.SetGridVisibility(false, false);
                }
            }
        }

        if (currentActiveUnit.role == CharacterRole.Player)
        {
            Debug.Log($"[LƯỢT ĐIỀU KHIỂN] Đến lượt của: {currentActiveUnit.unitName}.");
            if (handManager != null)
            {
                // Đẩy tay bài riêng của nhân vật này lên giao diện màn hình
                handManager.DrawRealCards(currentActiveUnit.itemHand, currentActiveUnit.skillHand);

                // Cập nhật các chỉ số máu, mana, AP thật của nhân vật này lên thanh HUD UI chính
                handManager.playerStats.currentAP = currentActiveUnit.currentAP;
                handManager.playerStats.currentMP = currentActiveUnit.currentMP;
                handManager.playerStats.currentHP = currentActiveUnit.currentHP;
                handManager.playerStats.maxHP = currentActiveUnit.baseData.combatStats.maxHP;
                handManager.playerStats.maxMP = currentActiveUnit.baseData.combatStats.maxMP;
                handManager.playerStats.UpdateUI();
            }
        }
        else
        {
            Debug.Log($"[LƯỢT TỰ ĐỘNG] {currentActiveUnit.unitName} đang tự động suy nghĩ...");
            if (handManager != null) handManager.HideHandUI(); // Ẩn UI bài đi để người chơi không nhấn bậy được
            if (aiManager != null) aiManager.ExecuteAITurn(currentActiveUnit, this);
        }
    }

    // ==========================================
    // HÀM KẾT THÚC LƯỢT (NÚT END) - ĐẨY SANG KHAY B
    // ==========================================
    public void PlayerEndTurn()
    {
        if (state == BattleState.NormalTurnPhase || state == BattleState.WaitTurnPhase)
        {
            currentActiveUnit.currentAGI = currentActiveUnit.baseData.combatStats.AGI;
            nextRoundQueue.Add(currentActiveUnit);
            NextTurn();
        }
    }

    // ==========================================
    // HÀM GẮN VÀO NÚT "WAIT / KHÔNG LÀM GÌ"
    // ==========================================
    public void OnWaitButtonClicked()
    {
        if (state == BattleState.NormalTurnPhase)
        {
            waitTurnQueue.Add(currentActiveUnit);

            // [FIX LỖI HEROES 3]: Sắp xếp lại ngay lập tức: AGI thấp đứng trước, AGI cao bị đẩy xuống sau cùng
            waitTurnQueue = waitTurnQueue.OrderBy(unit => unit.currentAGI).ToList();

            if (timelineManager != null) timelineManager.RebuildTimeline(null, normalTurnQueue, waitTurnQueue, nextRoundQueue);
            NextTurn();
        }
        else if (state == BattleState.WaitTurnPhase)
        {
            currentActiveUnit.currentAGI = currentActiveUnit.baseData.combatStats.AGI;
            nextRoundQueue.Add(currentActiveUnit);
            NextTurn();
        }
    }

    // ==========================================
    // XỬ LÝ TRỪ AGI KHI ĐÁNH BÀI (DELAY)
    // ==========================================
    public void ApplyActionDelay(int delayAGI)
    {
        currentActiveUnit.currentAGI -= delayAGI;
    }
}

// ==========================================
// THỰC THỂ SỐNG TRONG TRẬN ĐẤU (DATA + BỘ BÀI RIÊNG)
// ==========================================
[System.Serializable]
public class BattleUnit
{
    public string unitName;
    public CharacterRole role;
    public int currentHP; public int currentMP; public int currentAP; public int currentAGI;
    public CharacterStats baseData;

    // Biến lưu trữ Model 3D ngoài Scene (để Camera & Grid bắt mục tiêu vật lý)
    public Transform unitTransform;

    // Bộ bài cá nhân lưu trữ động trong trận của riêng thực thể này
    public List<ItemCardInstance> itemDrawPile = new List<ItemCardInstance>();
    public List<ItemCardInstance> itemHand = new List<ItemCardInstance>();
    public List<ItemCardInstance> itemDiscardPile = new List<ItemCardInstance>();

    public List<SkillCardInstance> skillDrawPile = new List<SkillCardInstance>();
    public List<SkillCardInstance> skillHand = new List<SkillCardInstance>();
    public List<SkillCardInstance> skillDiscardPile = new List<SkillCardInstance>();

    public BattleUnit(CharacterStats data)
    {
        baseData = data; unitName = data.characterName; role = data.role;
        currentHP = data.combatStats.maxHP; currentMP = data.combatStats.maxMP;
        currentAP = 3; currentAGI = data.combatStats.AGI;
    }
}
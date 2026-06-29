using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum BattleState { Start, DrawCards, NormalTurnPhase, WaitTurnPhase, Won, Lost }

public class BattleManager : MonoBehaviour
{
    [Header("--- THÔNG TIN TRẬN ĐẤU ---")]
    public BattleState state;

    [Header("--- KẾT NỐI UI CHÍNH ---")]
    public HandManager handManager;

    [Header("--- KẾT NỐI UI LƯỢT ĐI ---")]
    public TurnTimelineManager timelineManager;

    [Header("--- KẾT NỐI PANEL STATUS (TURN COUNTER) ---")]
    public AnomalyAndTurnStatusPanelUI statusUI;
    public int currentRoundCount = 1;

    [Header("--- KẾT NỐI NGƯỜI QUẢN LÝ DỊ THƯỜNG ---")]
    public AnomalyManager anomalyManager;

    [Header("--- KẾT NỐI NGƯỜI QUẢN LÝ AI ---")]
    public BattleAIManager aiManager; // Kéo cục Battle_AI_System vào đây

    [Header("--- DANH SÁCH THAM CHIẾN ---")]
    public List<CharacterStats> playerTeamData;
    public List<CharacterStats> enemyTeamData;

    [Header("--- DỮ LIỆU BỘ BÀI MANG VÀO TRẬN ---")]
    public DeckData playerDeckData;

    [Header("--- LUẬT BỐC BÀI ---")]
    public int baseItemDrawAmount = 3;
    public int baseSkillDrawAmount = 3;

    [Header("--- QUẢN LÝ THẺ ITEM ---")]
    public List<ItemCardInstance> itemDrawPile = new List<ItemCardInstance>();
    public List<ItemCardInstance> itemHand = new List<ItemCardInstance>();
    public List<ItemCardInstance> itemDiscardPile = new List<ItemCardInstance>();

    [Header("--- QUẢN LÝ THẺ SKILL ---")]
    public List<SkillCardInstance> skillDrawPile = new List<SkillCardInstance>();
    public List<SkillCardInstance> skillHand = new List<SkillCardInstance>();
    public List<SkillCardInstance> skillDiscardPile = new List<SkillCardInstance>();

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
        foreach (var data in playerTeamData) allUnitsInBattle.Add(new BattleUnit(data));
        foreach (var data in enemyTeamData) allUnitsInBattle.Add(new BattleUnit(data));

        InitializeDeck();

        if (anomalyManager != null) anomalyManager.InitializeAnomaliesOnStart();

        StartNewRound();
    }

    void InitializeDeck()
    {
        if (playerDeckData == null) return;
        foreach (ItemCardData itemData in playerDeckData.activeItemCards)
            itemDrawPile.Add(new ItemCardInstance(itemData, itemData.maxDurability));

        foreach (SkillCardData skillData in playerDeckData.activeSkillCards)
            skillDiscardPile.Add(new SkillCardInstance(skillData));

        ShuffleList(itemDrawPile); ShuffleList(skillDrawPile);
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
        if (state != BattleState.Start)
        {
            currentRoundCount++;
        }
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

        if (handManager != null && handManager.playerStats != null)
        {
            handManager.playerStats.currentAP = 3;
            handManager.playerStats.UpdateUI();
        }

        state = BattleState.DrawCards;
        DrawPhase();
    }

    void DrawPhase()
    {
        DiscardEntireHand();
        DrawItemCards(baseItemDrawAmount);
        DrawSkillCards(baseSkillDrawAmount);

        if (handManager != null) handManager.DrawRealCards(itemHand, skillHand);
        state = BattleState.NormalTurnPhase;
        NextTurn();
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
            Debug.Log("HẾT TURN 1! XÓA VẠCH NGĂN CÁCH VÀ BẮT ĐẦU TURN 2...");
            StartNewRound();
            return;
        }

        if (timelineManager != null) timelineManager.RebuildTimeline(currentActiveUnit, normalTurnQueue, waitTurnQueue, nextRoundQueue);

        // 💡 MỚI THÊM: ĐIỀU PHỐI QUYỀN ĐIỀU KHIỂN (PLAYER VS AI)
        HandleUnitTurnStart();
    }

    // Hàm kiểm tra vai trò thực thể khi vào lượt
    void HandleUnitTurnStart()
    {
        if (currentActiveUnit == null) return;

        if (currentActiveUnit.role == CharacterRole.Player)
        {
            Debug.Log($"[LƯỢT ĐIỀU KHIỂN] Đến lượt của: {currentActiveUnit.unitName}. Mở UI cho người chơi!");
        }
        else
        {
            Debug.Log($"[LƯỢT TỰ ĐỘNG] {currentActiveUnit.unitName} ({currentActiveUnit.role}) đang tự động suy nghĩ...");
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
            Debug.Log($"[END] {currentActiveUnit.unitName} kết thúc lượt. Bị đẩy sang Turn 2!");

            currentActiveUnit.currentAGI = currentActiveUnit.baseData.combatStats.AGI;
            currentActiveUnit.currentAP = 3;

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
            Debug.Log($"{currentActiveUnit.unitName} chọn WAIT! Lùi xuống cuối hiệp.");
            waitTurnQueue.Add(currentActiveUnit);

            // 💡 [FIX HEROES 3]: Ép sắp xếp ngay lập tức: AGI thấp đứng trước, AGI cao bị đẩy xuống sau cùng
            waitTurnQueue = waitTurnQueue.OrderBy(unit => unit.currentAGI).ToList();

            if (timelineManager != null) timelineManager.RebuildTimeline(null, normalTurnQueue, waitTurnQueue, nextRoundQueue);

            NextTurn();
        }
        else if (state == BattleState.WaitTurnPhase)
        {
            currentActiveUnit.currentAGI = currentActiveUnit.baseData.combatStats.AGI;
            currentActiveUnit.currentAP = 3;
            nextRoundQueue.Add(currentActiveUnit);

            NextTurn();
        }
    }

    void DrawItemCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (itemDrawPile.Count == 0)
            {
                if (itemDiscardPile.Count == 0) break;
                itemDrawPile.AddRange(itemDiscardPile); itemDiscardPile.Clear(); ShuffleList(itemDrawPile);
            }
            itemHand.Add(itemDrawPile[0]); itemDrawPile.RemoveAt(0);
        }
    }

    void DrawSkillCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (skillDrawPile.Count == 0)
            {
                if (skillDiscardPile.Count == 0) break;
                skillDrawPile.AddRange(skillDiscardPile); skillDiscardPile.Clear(); ShuffleList(skillDrawPile);
            }
            skillHand.Add(skillDrawPile[0]); skillDrawPile.RemoveAt(0);
        }
    }

    void DiscardEntireHand()
    {
        itemDiscardPile.AddRange(itemHand); itemHand.Clear();
        skillDiscardPile.AddRange(skillHand); skillHand.Clear();
    }

    // ==========================================
    // XỬ LÝ TRỪ AGI KHI ĐÁNH BÀI
    // ==========================================
    public void ApplyActionDelay(int delayAGI)
    {
        currentActiveUnit.currentAGI -= delayAGI;
        Debug.Log($"{currentActiveUnit.unitName} dùng bài mất {delayAGI} AGI. AGI hiện tại: {currentActiveUnit.currentAGI}");
    }
}

// ==========================================
// THỰC THỂ SỐNG TRONG TRẬN ĐẤU
// ==========================================
[System.Serializable]
public class BattleUnit
{
    public string unitName;
    public CharacterRole role;
    public int currentHP; public int currentMP; public int currentAP; public int currentAGI;
    public CharacterStats baseData;

    public BattleUnit(CharacterStats data)
    {
        baseData = data; unitName = data.characterName; role = data.role;
        currentHP = data.combatStats.maxHP; currentMP = data.combatStats.maxMP;
        currentAP = 3; currentAGI = data.combatStats.AGI;
    }
}
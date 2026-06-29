using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{
    private BattleManager battleManager;

    [Header("--- THIẾT LẬP CƠ BẢN ---")]
    public GameObject cardPrefab;
    public Transform itemHandPanel;
    public Transform skillHandPanel;
    public CardDetailManager detailManager;
    public PlayerStats playerStats;

    [Header("--- KẾT NỐI UI LỆNH (MOVE/WAIT/RUN) ---")]
    public BattleUIManager_A uiManager;

    [Header("--- KẾT NỐI HỆ THỐNG TOKEN ---")]
    public TokenManager tokenManager;

    [Header("--- KẾT NỐI UI LƯỢT ĐI ---")]
    public TurnTimelineManager timelineManager;

    // QUẢN LÝ LÁ BÀI ĐANG CHỌN
    public CardDisplay currentSelectedCard = null;
    private bool isAnimatingAvatar = false;
    private List<CardDisplay> activeCardsInHand = new List<CardDisplay>();

    [HideInInspector] public int dropCardFrame = -1; // 💡 Ghi nhớ Frame (Khung hình) vừa vứt bài

    void Start()
    {
        if (timelineManager == null)
        {
            timelineManager = FindAnyObjectByType<TurnTimelineManager>();
        }

        battleManager = FindAnyObjectByType<BattleManager>();
    }

    void Update()
    {
        // 💡 ĐÃ SỬA: Đồng bộ dùng wasReleasedThisFrame (Lúc thả chuột ra)
        if (Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            if (currentSelectedCard != null)
            {
                dropCardFrame = Time.frameCount; // Báo hiệu: "Tôi vừa vứt bài ở Khung hình này!"
            }
            DeselectAllCards();
        }
    }

    // ==========================================
    // LOGIC CHỌN BÀI (UX MỚI)
    // ==========================================
    public void SelectCard(CardDisplay card)
    {
        currentSelectedCard = card;
        card.RaiseCard();
        card.ShowDetailPanel();

        RefreshHandVisuals();
    }

    public void DeselectAllCards()
    {
        if (currentSelectedCard != null)
        {
            currentSelectedCard.LowerCard();
            currentSelectedCard = null;
        }
        if (detailManager != null) detailManager.HideDetailPanel();

        RefreshHandVisuals();
    }

    public void RefreshHandVisuals()
    {
        if (playerStats == null) return;
        foreach (CardDisplay card in activeCardsInHand)
        {
            card.CheckAffordability(playerStats);
        }
    }

    public void PlayCard(CardDisplay cardToPlay)
    {
        if (playerStats == null || isAnimatingAvatar) return;

        int delayValue = 0;
        if (cardToPlay.currentItemData != null)
        {
            playerStats.SpendAP(cardToPlay.currentItemData.apCost);
            delayValue = cardToPlay.currentItemData.delayAGI;
            if (tokenManager != null) tokenManager.AddTokens(cardToPlay.currentItemData.tokensToAdd);
        }
        else if (cardToPlay.currentSkillData != null)
        {
            playerStats.SpendAP(cardToPlay.currentSkillData.apCost);
            playerStats.SpendMP(cardToPlay.currentSkillData.mpCost);
            delayValue = cardToPlay.currentSkillData.delayAGI;
            if (tokenManager != null) tokenManager.AddTokens(cardToPlay.currentSkillData.tokensToAdd);
        }

        cardToPlay.gameObject.SetActive(false);
        currentSelectedCard = null;
        if (detailManager != null) detailManager.HideDetailPanel();

        if (timelineManager != null)
        {
            if (uiManager != null) uiManager.MarkCardPlayed();

            isAnimatingAvatar = true;

            timelineManager.AnimateAvatarDrop(delayValue, () =>
            {
                activeCardsInHand.Remove(cardToPlay);
                Destroy(cardToPlay.gameObject);

                if (battleManager != null) battleManager.ApplyActionDelay(delayValue);

                isAnimatingAvatar = false;
                StartCoroutine(RebuildLayoutNextFrame());
            });
        }
    }

    public void DrawRealCards(List<ItemCardInstance> realItemHand, List<SkillCardInstance> realSkillHand)
    {
        foreach (Transform child in itemHandPanel) Destroy(child.gameObject);
        foreach (Transform child in skillHandPanel) Destroy(child.gameObject);
        activeCardsInHand.Clear();
        currentSelectedCard = null;
        foreach (ItemCardInstance itemInst in realItemHand)
        {
            GameObject newCard = Instantiate(cardPrefab, itemHandPanel);
            CardDisplay display = newCard.GetComponent<CardDisplay>();
            display.SetupItemCard(itemInst.baseData, detailManager, this);
            activeCardsInHand.Add(display);
        }

        foreach (SkillCardInstance skillInst in realSkillHand)
        {
            GameObject newCard = Instantiate(cardPrefab, skillHandPanel);
            CardDisplay display = newCard.GetComponent<CardDisplay>();
            display.SetupSkillCard(skillInst.baseData, detailManager, this);
            activeCardsInHand.Add(display);
        }
        StartCoroutine(RebuildLayoutNextFrame());
    }

    IEnumerator RebuildLayoutNextFrame()
    {
        itemHandPanel.GetComponent<CanvasGroup>().alpha = 0; skillHandPanel.GetComponent<CanvasGroup>().alpha = 0;
        yield return null; yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemHandPanel.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(skillHandPanel.GetComponent<RectTransform>());
        itemHandPanel.GetComponent<CanvasGroup>().alpha = 1; skillHandPanel.GetComponent<CanvasGroup>().alpha = 1;
        RefreshHandVisuals();
    }
}
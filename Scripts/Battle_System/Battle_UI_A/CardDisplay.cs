using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("--- KẾT NỐI UI ---")]
    public Image cardArt;
    public Image cardFrame;
    public Image leftIcon;
    public Image rightIcon;
    public TextMeshProUGUI rightText;

    [Header("--- ICON ---")]
    public Sprite gearSprite;
    public Sprite manaSprite;

    [Header("--- DỮ LIỆU ---")]
    public ItemCardData currentItemData;
    public SkillCardData currentSkillData;

    private CardDetailManager detailManager;
    private HandManager handManager;
    private MouseControllerMoveButton mouseCtrl;

    [Header("--- ANIMATION (CHỈNH TRÊN INSPECTOR) ---")]
    public float hoverScale = 1.15f; // Độ phình to khi hơ chuột
    public float selectRaiseY = 50f; // Độ cao khi nhấc bài

    private Vector3 originalScale;
    private Vector2 originalPosition;

    public bool isAffordable = true;
    public bool isRaised = false;

    private Canvas cardCanvas;
    private int defaultSortingOrder = 0;

    void Start()
    {
        originalScale = transform.localScale;
        cardCanvas = GetComponent<Canvas>();

        // Nhớ bộ quản lý chuột để check trạng thái lockedEnemy liên tục
        mouseCtrl = Object.FindAnyObjectByType<MouseControllerMoveButton>();

        if (cardCanvas != null)
        {
            // Thiết lập mặc định để bài bên phải đè lên bài bên trái một chút
            defaultSortingOrder = 100 - transform.GetSiblingIndex();
            cardCanvas.sortingOrder = defaultSortingOrder;
            cardCanvas.overrideSorting = true;
        }
    }

    // 💡 THEO DÕI LIÊN TỤC: Cập nhật màu sẫm/sáng của lá bài theo thời gian thực (Real-time)
    void Update()
    {
        UpdateVisualState();
    }

    public void SetupItemCard(ItemCardData data, CardDetailManager manager, HandManager hManager)
    {
        currentItemData = data; currentSkillData = null; detailManager = manager; handManager = hManager;
        cardArt.sprite = data.cardArt; cardFrame.sprite = data.frameArt;
        rightIcon.sprite = gearSprite; rightIcon.rectTransform.sizeDelta = new Vector2(50, 50);
        rightText.text = $"<color=#FFFFFF>{data.maxDurability}</color>";
        if (data.typeIcon != null) { leftIcon.sprite = data.typeIcon; leftIcon.color = Color.white; }
        else leftIcon.color = new Color(1, 1, 1, 0);
    }

    public void SetupSkillCard(SkillCardData data, CardDetailManager manager, HandManager hManager)
    {
        currentSkillData = data; currentItemData = null; detailManager = manager; handManager = hManager;
        cardArt.sprite = data.cardArt; cardFrame.sprite = data.frameArt;
        rightIcon.sprite = manaSprite; rightIcon.rectTransform.sizeDelta = new Vector2(70, 70);
        rightText.text = $"<color=#00FFFF>{data.mpCost}</color>";
        if (data.typeIcon != null) { leftIcon.sprite = data.typeIcon; leftIcon.color = Color.white; }
        else leftIcon.color = new Color(1, 1, 1, 0);
    }

    public void CheckAffordability(PlayerStats playerStats)
    {
        if (currentItemData != null) isAffordable = playerStats.CanAffordAP(currentItemData.apCost);
        else if (currentSkillData != null) isAffordable = playerStats.CanAffordAP(currentSkillData.apCost) && playerStats.CanAffordMP(currentSkillData.mpCost);
        UpdateVisualState();
    }

    // =========================================================================
    // 💡 THUẬT TOÁN ĐỔI MÀU: ƯU TIÊN KIỂM TRA TARGET TRƯỚC TIÊN
    // =========================================================================
    public void UpdateVisualState()
    {
        Color tintColor = Color.white;

        // Kiểm tra xem người chơi đã Target quái vật chưa
        bool hasTarget = (mouseCtrl != null && mouseCtrl.lockedEnemy != null);

        if (!hasTarget)
        {
            // 1. CHƯA TARGET ENEMY -> Ép bài sẫm màu lại lập tức (Mức độ tối xám sâu 0.25f)
            tintColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        }
        else if (!isAffordable)
        {
            // 2. Đã có target nhưng không đủ AP/MP để đánh
            tintColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        }
        else if (handManager != null && handManager.currentSelectedCard != null && handManager.currentSelectedCard != this)
        {
            // 3. Đang chọn 1 lá bài khác nhấc lên, các lá còn lại sẫm màu nhẹ
            tintColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        // Đổ màu thực tế lên hình ảnh lá bài
        if (cardArt != null) cardArt.color = tintColor;
        if (cardFrame != null) cardFrame.color = tintColor;
    }

    public void RaiseCard()
    {
        if (isRaised) return;
        originalPosition = GetComponent<RectTransform>().anchoredPosition;
        isRaised = true;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(originalPosition.x, originalPosition.y + selectRaiseY);
        if (cardCanvas != null) cardCanvas.sortingOrder = defaultSortingOrder + 50;

        if (handManager != null && handManager.timelineManager != null)
        {
            int delayValue = currentItemData != null ? currentItemData.delayAGI : currentSkillData.delayAGI;
            handManager.timelineManager.ShowPrediction(delayValue);
        }

        if (mouseCtrl != null) mouseCtrl.SetHoldingCard(true);
    }

    public void LowerCard()
    {
        if (!isRaised) return;
        isRaised = false;
        GetComponent<RectTransform>().anchoredPosition = originalPosition;
        if (cardCanvas != null) cardCanvas.sortingOrder = defaultSortingOrder;

        if (handManager != null && handManager.timelineManager != null)
        {
            handManager.timelineManager.HidePrediction();
        }

        if (mouseCtrl != null) mouseCtrl.SetHoldingCard(false);
    }

    public void ShowDetailPanel()
    {
        if (detailManager == null) return;
        if (currentItemData != null) detailManager.ShowItemCardDetails(currentItemData);
        else if (currentSkillData != null) detailManager.ShowSkillCardDetails(currentSkillData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isRaised) originalPosition = GetComponent<RectTransform>().anchoredPosition;
        bool isAnotherCardSelected = (handManager != null && handManager.currentSelectedCard != null && handManager.currentSelectedCard != this);

        // Chỉ cho phép tương tác hơ chuột nếu ĐÃ CÓ TARGET
        bool hasTarget = (mouseCtrl != null && mouseCtrl.lockedEnemy != null);

        if (isAffordable && !isAnotherCardSelected && hasTarget)
        {
            // LÀM BÀI PHÌNH TO:
            transform.localScale = originalScale * hoverScale;

            if (handManager != null && handManager.timelineManager != null)
            {
                int delayValue = currentItemData != null ? currentItemData.delayAGI : currentSkillData.delayAGI;
                handManager.timelineManager.ShowPrediction(delayValue);
            }
        }

        if (cardCanvas != null) cardCanvas.sortingOrder = defaultSortingOrder + 100;
        if (handManager != null && handManager.currentSelectedCard != null) ShowDetailPanel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        if (!isRaised && cardCanvas != null) cardCanvas.sortingOrder = defaultSortingOrder;

        if (!isRaised && handManager != null && handManager.timelineManager != null)
        {
            if (handManager.currentSelectedCard == null)
                handManager.timelineManager.HidePrediction();
        }

        if (handManager != null && handManager.currentSelectedCard != null) handManager.currentSelectedCard.ShowDetailPanel();
        else if (detailManager != null) detailManager.HideDetailPanel();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 💡 CHẶN HOÀN TOÀN: Nếu không đủ tiền HOẶC chưa chọn target thì cấm Click chọn bài
            bool hasTarget = (mouseCtrl != null && mouseCtrl.lockedEnemy != null);
            if (!isAffordable || !hasTarget) return;

            if (handManager.currentSelectedCard == null) handManager.SelectCard(this);
            else if (handManager.currentSelectedCard == this) handManager.PlayCard(this);
        }
    }
}
using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailManager : MonoBehaviour
{
    [Header("--- KẾT NỐI UI ---")]
    public TextMeshProUGUI infoText;
    public Button closeButton;

    [Header("--- HỆ THỐNG CUỘN CHỮ ---")]
    public RectTransform textContent; // Kéo Info_Text vào đây
    public float scrollSpeed = 300f;  // Tốc độ cuộn

    [Header("--- VỊ TRÍ HIỂN THỊ ---")]
    public Vector2 posForItems = new Vector2(400, 200);   // Hiện bên Phải khi bấm Item
    public Vector2 posForSkills = new Vector2(-400, 200); // Hiện bên Trái khi bấm Skill

    private RectTransform myRect;
    private bool isScrollingUp = false;
    private bool isScrollingDown = false;

    void Awake()
    {
        myRect = GetComponent<RectTransform>();
    }

    void Start()
    {
        closeButton.onClick.AddListener(HideDetailPanel);
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Xử lý cuộn chữ mượt mà khi giữ nút
        if (isScrollingUp || isScrollingDown)
        {
            Vector2 currentPos = textContent.anchoredPosition;
            RectTransform viewport = (RectTransform)textContent.parent;
            float maxScroll = Mathf.Max(0, textContent.rect.height - viewport.rect.height);

            if (isScrollingUp) currentPos.y -= scrollSpeed * Time.deltaTime;
            else if (isScrollingDown) currentPos.y += scrollSpeed * Time.deltaTime;

            currentPos.y = Mathf.Clamp(currentPos.y, 0, maxScroll);
            textContent.anchoredPosition = currentPos;
        }
    }

    public void StartScrollUp() { isScrollingUp = true; }
    public void StopScrollUp() { isScrollingUp = false; }
    public void StartScrollDown() { isScrollingDown = true; }
    public void StopScrollDown() { isScrollingDown = false; }

    // ==========================================
    // HÀM HIỂN THỊ DÀNH CHO ITEM CARD
    // ==========================================
    public void ShowItemCardDetails(ItemCardData itemData)
    {
        // 1. Nhảy vị trí và Tên bài
        myRect.anchoredPosition = posForItems;
        string finalHtmlText = $"<size=45><b><color=#3E2723>{itemData.cardName}</color></b></size>\n";

        // 2. Dòng hiển thị Độ bền
        finalHtmlText += $"<i><color=#555555>(Độ bền còn lại: {itemData.maxDurability})</color></i>\n\n";

        // 3. Các chỉ số cơ bản
        if (!string.IsNullOrEmpty(itemData.apCostText))
            finalHtmlText += $"<b><color=#3E2723>AP Tiêu hao:</color></b> <color=#3E2723>{itemData.apCostText}</color>\n\n";

        if (!string.IsNullOrEmpty(itemData.positiveEffect))
            finalHtmlText += $"<b><color=#3E2723>Tác dụng:</color></b> <color=#006400>{itemData.positiveEffect}</color>\n\n";

        if (!string.IsNullOrEmpty(itemData.negativeEffect))
            finalHtmlText += $"<b><color=#3E2723>Tác hại:</color></b> <color=#8B0000>{itemData.negativeEffect}</color>\n\n";

        if (!string.IsNullOrEmpty(itemData.rangeText))
            finalHtmlText += $"<b><color=#3E2723>Phạm vi:</color></b> <color=#00008B>{itemData.rangeText}</color>\n\n";

        if (!string.IsNullOrEmpty(itemData.tokenGenerationText))
            finalHtmlText += $"<b><color=#3E2723>Tạo Token:</color></b> <color=#00008B>{itemData.tokenGenerationText}</color>\n\n";

            finalHtmlText += $"<b><color=#3E2723>Ảnh hưởng độ trễ:</color></b> <color=#B22222>-{itemData.delayAGI} AGI</color>\n\n";

        // 4. KẾT THÚC TRẬN nằm trên
        if (!string.IsNullOrEmpty(itemData.endBattleEffect))
            finalHtmlText += $"<b><color=#3E2723>Kết thúc trận:</color></b> <color=#4B0082>{itemData.endBattleEffect}</color>\n\n";

        // 5. NGUYÊN LIỆU CHẾ TẠO nằm kế tiếp
        if (!string.IsNullOrEmpty(itemData.craftingRecipeText))
            finalHtmlText += $"<b><color=#3E2723>Nguyên liệu chế tạo:</color></b> <color=#663300>{itemData.craftingRecipeText}</color>\n\n";

        // 6. CÂU CHUYỆN (Flavor text) nằm dưới cùng
        if (!string.IsNullOrEmpty(itemData.flavorText))
            finalHtmlText += $"<i><color=#555555>\"{itemData.flavorText}\"</color></i>";

        infoText.text = finalHtmlText;
        textContent.anchoredPosition = new Vector2(textContent.anchoredPosition.x, 0);
        gameObject.SetActive(true);
    }

    // ==========================================
    // HÀM HIỂN THỊ DÀNH CHO SKILL CARD
    // ==========================================
    public void ShowSkillCardDetails(SkillCardData skillData)
    {
        myRect.anchoredPosition = posForSkills;
        string finalHtmlText = $"<size=45><b><color=#3E2723>{skillData.cardName}</color></b></size>\n";

        finalHtmlText += $"<i><color=#555555>(MP tiêu hao: {skillData.mpCost})</color></i>\n\n";

        if (!string.IsNullOrEmpty(skillData.apCostText))
            finalHtmlText += $"<b><color=#3E2723>Tiêu hao:</color></b> <color=#3E2723>{skillData.apCostText}</color>\n\n";

        if (!string.IsNullOrEmpty(skillData.positiveEffect))
            finalHtmlText += $"<b><color=#3E2723>Tác dụng:</color></b> <color=#006400>{skillData.positiveEffect}</color>\n\n";

        if (!string.IsNullOrEmpty(skillData.negativeEffect))
            finalHtmlText += $"<b><color=#3E2723>Tác hại:</color></b> <color=#8B0000>{skillData.negativeEffect}</color>\n\n";

        if (!string.IsNullOrEmpty(skillData.rangeText))
            finalHtmlText += $"<b><color=#3E2723>Phạm vi:</color></b> <color=#00008B>{skillData.rangeText}</color>\n\n";

        if (!string.IsNullOrEmpty(skillData.tokenConsumptionText))
            finalHtmlText += $"<b><color=#3E2723>Tiêu hao Token:</color></b> <color=#00008B>{skillData.tokenConsumptionText}</color>\n\n";

        if (!string.IsNullOrEmpty(skillData.tokenGenerationText))
            finalHtmlText += $"<b><color=#3E2723>Tạo Token:</color></b> <color=#00008B>{skillData.tokenGenerationText}</color>\n\n";

            finalHtmlText += $"<b><color=#3E2723>Ảnh hưởng độ trễ:</color></b> <color=#B22222>-{skillData.delayAGI} AGI</color>\n\n";

        // Đảo thứ tự hệt như Item Card
        if (!string.IsNullOrEmpty(skillData.endBattleEffect))
            finalHtmlText += $"<b><color=#3E2723>Kết thúc trận:</color></b> <color=#4B0082>{skillData.endBattleEffect}</color>\n\n";

        if (!string.IsNullOrEmpty(skillData.craftingRecipeText))
            finalHtmlText += $"<b><color=#3E2723>Nguyên liệu chế tạo:</color></b> <color=#663300>{skillData.craftingRecipeText}</color>\n\n";

        if (!string.IsNullOrEmpty(skillData.flavorText))
            finalHtmlText += $"<i><color=#555555>\"{skillData.flavorText}\"</color></i>";

        infoText.text = finalHtmlText;
        textContent.anchoredPosition = new Vector2(textContent.anchoredPosition.x, 0);
        gameObject.SetActive(true);
    }


    public void HideDetailPanel()
    {
        gameObject.SetActive(false);
    }
}

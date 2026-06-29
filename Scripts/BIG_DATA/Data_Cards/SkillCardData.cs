using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Skill Card", menuName = "JRPG System/Cards/Skill Card")]
public class SkillCardData : ScriptableObject
{
    [Header("--- THÔNG TIN CƠ BẢN ---")]
    public string cardName = "Tên Kỹ Năng";

    [Header("--- VĂN BẢN HIỂN THỊ (UI) ---")]
    [Tooltip("Thường là 0, bỏ trống sẽ tự ẩn")]
    [TextArea(1, 2)] public string apCostText = "";

    [TextArea(2, 3)] public string positiveEffect = "Tác dụng: ...";

    [Tooltip("Bỏ trống sẽ tự ẩn")]
    [TextArea(2, 3)] public string negativeEffect = "";

    [Tooltip("Bỏ trống sẽ tự ẩn")]
    [TextArea(1, 2)] public string rangeText = "";

    [Tooltip("Bỏ trống sẽ tự ẩn")]
    [TextArea(1, 2)] public string tokenConsumptionText = "";

    [Tooltip("(Ne) Bỏ trống sẽ tự ẩn")]
    [TextArea(1, 2)] public string tokenGenerationText = "";

    [Tooltip("Số AGI bị trừ làm lùi lượt khi dùng lá bài này")]
    public int delayAGI = 5;

    [Tooltip("Hiệu ứng khi kết thúc trận. Bỏ trống sẽ tự ẩn")]
    [TextArea(2, 3)] public string endBattleEffect = "";

    [Tooltip("Điều kiện học / Nguyên liệu nâng cấp. Bỏ trống sẽ tự ẩn")]
    [TextArea(1, 2)] public string craftingRecipeText = "";

    [Tooltip("Câu chuyện / Mô tả kỹ năng (In nghiêng). Bỏ trống sẽ tự ẩn")]
    [TextArea(2, 3)] public string flavorText = "";

    [Header("--- HÌNH ẢNH HIỂN THỊ (UI) ---")]
    public Sprite cardArt;    // Kéo ảnh minh họa kỹ năng vào đây
    public Sprite frameArt;   // Kéo khung viền (Xanh lam / Viền Lửa...) vào đây
    public Sprite typeIcon;   // Kéo Icon hệ (Lửa/Nước...) vào góc trái

    [Header("--- CHỈ SỐ TIÊU HAO ---")]
    public int apCost = 2;    // Dùng kỹ năng thường tốn nhiều AP hơn Item
    public int mpCost = 15;

    [Header("--- LUẬT ĐẶC BIỆT ---")]
    [Tooltip("Bốc trúng lá này là tự động kích hoạt lập tức. Có thể là Phước lành, hoặc Bẫy/Nguyền rủa trừ thẳng AP/MP của người chơi!")]
    public bool isAutoPlayOnDraw = false;

    [Header("--- ĐIỀU KIỆN KÍCH HOẠT COMBO ---")]
    public List<TokenData> requiredTokens;
    public bool consumeTokensOnUse = true;

    [Header("--- HIỆU ỨNG TẠO RA SAU KHI DÙNG ---")]
    public List<TokenData> tokensToAdd;

    [Header("--- KỸ NĂNG PHỐI HỢP ĐỒNG ĐỘI ---")]
    public bool isLinkSkill = false;
    public Sprite linkedCharacterIcon;
}
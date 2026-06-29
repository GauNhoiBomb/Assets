using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Item Card", menuName = "JRPG System/Cards/Item Card")]
public class ItemCardData : ScriptableObject
{
    [Header("--- THÔNG TIN CƠ BẢN ---")]
    public string cardName = "Tên Vật Phẩm";

    [Header("--- VĂN BẢN HIỂN THỊ (UI) ---")]
    [TextArea(1, 2)] public string apCostText = "AP tiêu hao: 1";

    [TextArea(2, 3)] public string positiveEffect = "Tác dụng: ...";

    [Tooltip("Bỏ trống sẽ tự động ẩn đi")]
    [TextArea(2, 3)] public string negativeEffect = "";

    [Tooltip("Cận chiến / Tầm xa... Bỏ trống sẽ tự ẩn")]
    [TextArea(1, 2)] public string rangeText = "";

    [Tooltip("Bỏ trống sẽ tự ẩn")]
    [TextArea(1, 2)] public string tokenGenerationText = "";

    [Tooltip("Số AGI bị trừ làm lùi lượt khi dùng lá bài này")]
    public int delayAGI = 5;

    [Tooltip("Hiệu ứng khi kết thúc trận (VD: Hồi lại 1 độ bền). Bỏ trống sẽ tự ẩn")]
    [TextArea(2, 3)] public string endBattleEffect = "";

    [Tooltip("Công thức chế tạo. Bỏ trống sẽ tự ẩn")]
    [TextArea(1, 2)] public string craftingRecipeText = "";

    [Tooltip("Câu chuyện / Mô tả món đồ (In nghiêng). Bỏ trống sẽ tự ẩn")]
    [TextArea(2, 3)] public string flavorText = "";

    [Header("--- HÌNH ẢNH HIỂN THỊ (UI) ---")]
    public Sprite cardArt;    // Kéo ảnh Vũ khí / Vật phẩm vào đây
    public Sprite frameArt;   // Kéo khung viền (Xám/Xanh/Vàng) vào đây
    public Sprite typeIcon;   // Kéo Icon góc trái (Đâm tam giác/Đập ngũ giác...) vào đây

    [Header("--- CHỈ SỐ TIÊU HAO ---")]
    public int apCost = 1;
    public int maxDurability = 60;

    [Header("--- LUẬT ĐẶC BIỆT ---")]
    [Tooltip("Vũ khí gãy (Độ bền = 0) vẫn được dùng để lấy Token, nhưng không có Animation hay Sát thương?")]
    public bool usableWhenBroken = true;

    [Header("--- ĐIỀU KIỆN KÍCH HOẠT COMBO ---")]
    public List<TokenData> requiredTokens;
    public bool consumeTokensOnUse = true;

    [Header("--- HIỆU ỨNG TẠO RA SAU KHI DÙNG ---")]
    public List<TokenData> tokensToAdd;
}
using UnityEngine;

// ==========================================
// THỰC THỂ LÁ BÀI VẬT PHẨM TRONG TRẬN ĐẤU
// ==========================================
[System.Serializable]
public class ItemCardInstance
{
    [Tooltip("Bản thiết kế gốc của lá bài (Chứa ảnh, tên, mô tả)")]
    public ItemCardData baseData;

    [Tooltip("Độ bền hiện tại thay đổi liên tục trong trận (Ví dụ: 53/60)")]
    public int currentDurability;

    // node: Hàm này được gọi khi rút lá bài từ ngoài Inventory vào Trận đấu
    // Bạn truyền cây giáo 60 độ bền vào, nó sẽ lưu thành currentDurability = 60
    // (Nếu truyền cây giáo cũ 53 độ bền, nó sẽ nhận 53. Tính năng này ta sẽ code khi kết nối Inventory sau).
    public ItemCardInstance(ItemCardData data, int durabilityToSet)
    {
        baseData = data;
        currentDurability = durabilityToSet;
    }

    // node: Hàm gọi khi xài bài. Nếu đồ gãy (0 độ bền) thì không cho xài (trừ vũ khí có luật unusableWhenBroken)
    public void UseCard()
    {
        if (currentDurability > 0)
        {
            currentDurability--;
            Debug.Log($"Đã dùng {baseData.cardName}. Độ bền còn: {currentDurability}");
        }
        else
        {
            Debug.Log($"{baseData.cardName} đã gãy, chỉ lấy được Token!");
        }
    }
}

// ==========================================
// THỰC THỂ LÁ BÀI KỸ NĂNG TRONG TRẬN ĐẤU
// ==========================================
[System.Serializable]
public class SkillCardInstance
{
    public SkillCardData baseData;

    // node: Skill không có độ bền, nên ta chỉ cần bọc Bản thiết kế gốc lại là xong!
    public SkillCardInstance(SkillCardData data)
    {
        baseData = data;
    }
}
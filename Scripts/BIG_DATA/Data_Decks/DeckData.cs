using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Player Deck", menuName = "JRPG System/Deck Data")]
public class DeckData : ScriptableObject
{
    [Header("--- TRẠNG THÁI CỐT TRUYỆN ---")]
    // node: Biến này sẽ TẮT ở đầu game (Tutorial). 
    // Khi người chơi qua màn cốt truyện, bạn (hoặc Visual Scripting) chỉ cần bật nó lên thành TRUE.
    [Tooltip("Lời nguyền có đang kích hoạt không? (Tắt khi đang ở Tutorial)")]
    public bool isCurseActive = false;

    [Header("--- LUẬT BỘ BÀI ---")]
    // node: Quy định số lượng tối thiểu theo Game Design của bạn
    public int minItemsRequired = 10;
    public int minSkillsRequired = 10;

    // node: Số % chỉ số bị trừ cho MỖI lá bài bị thiếu
    [Tooltip("Số % bị trừ cho mỗi lá bài thiếu (Mặc định 10%)")]
    public float penaltyPerMissingCard = 10f;

    [Header("--- BỘ BÀI HIỆN TẠI MANG VÀO TRẬN ---")]
    // node: Đây là nơi người chơi (hoặc UI Menu) sẽ ném các lá bài họ chọn vào
    public List<ItemCardData> activeItemCards = new List<ItemCardData>();
    public List<SkillCardData> activeSkillCards = new List<SkillCardData>();

    // ==========================================
    // HÀM TÍNH TOÁN HÌNH PHẠT (DÙNG KHI VÀO TRẬN ĐẤU)
    // ==========================================

    // node: Hàm này sẽ trả về TỔNG SỐ % bị trừ đi. 
    // Ví dụ: Trả về 20 nghĩa là bị trừ 20% mọi chỉ số. Trả về 0 nghĩa là an toàn.
    public float CalculateCursePenalty()
    {
        // 1. Nếu đang ở Tutorial (Lời nguyền chưa kích hoạt) -> An toàn tuyệt đối (0% phạt)
        if (!isCurseActive)
        {
            return 0f;
        }

        int totalMissingCards = 0;

        // 2. Đếm xem Item Card thiếu bao nhiêu lá so với yêu cầu (10)
        if (activeItemCards.Count < minItemsRequired)
        {
            totalMissingCards += (minItemsRequired - activeItemCards.Count);
        }

        // 3. Đếm xem Skill Card thiếu bao nhiêu lá so với yêu cầu (10)
        if (activeSkillCards.Count < minSkillsRequired)
        {
            totalMissingCards += (minSkillsRequired - activeSkillCards.Count);
        }

        // 4. Tính tổng % hình phạt (Ví dụ: Thiếu 3 lá * 10% = Phạt 30%)
        float totalPenalty = totalMissingCards * penaltyPerMissingCard;

        // node: Nếu người chơi "lầy lội" không mang lá nào (thiếu 20 lá), hình phạt có thể lên tới 200%.
        // Lệnh Mathf.Clamp đảm bảo hình phạt tối đa chỉ là 99% (để máu HP không bị âm).
        return Mathf.Clamp(totalPenalty, 0f, 99f);
    }

    // ==========================================
    // HÀM KIỂM TRA BỘ BÀI HỢP LỆ TRƯỚC KHI VÀO TRẬN
    // ==========================================
    // node: Bạn có thể dùng hàm này ở màn hình Chuẩn bị. 
    // Nếu nó hiện TRUE (thiếu bài), bạn có thể bật 1 cái bảng UI đỏ chót cảnh báo người chơi!
    public bool IsDeckUnderCurse()
    {
        return CalculateCursePenalty() > 0f;
    }
}
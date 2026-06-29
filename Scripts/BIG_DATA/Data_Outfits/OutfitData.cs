using UnityEngine;
using System.Collections.Generic;

public enum ElementType
{
    None, Fire, Water, Earth, Wind, Light, Dark
}

public enum NodeType
{
    MinorStat,  // Ô nhỏ (Cộng chỉ số cơ bản)
    MajorSkill  // Ô to (Mở khóa Tuyệt kỹ hoặc Chỉ số Quản lý đặc biệt)
}

[CreateAssetMenu(fileName = "New Outfit Data", menuName = "JRPG System/Outfit Data")]
public class OutfitData : ScriptableObject
{
    [Header("--- THÔNG TIN TRANG PHỤC ---")]
    public string outfitName = "Tên Trang Phục";
    public ElementType element = ElementType.None;

    [TextArea(2, 4)]
    public string description = "Mô tả bộ trang phục...";

    [Header("--- CHỈ SỐ GỐC (KHI VỪA MẶC VÀO) ---")]
    // node: Điền số âm (Ví dụ -1 MOV) vào đây thoải mái để làm đồ nguyền rủa / giáp nặng
    public CombatStats baseCombatBonus;

    // node: Phần thưởng quản lý cộng ngay khi mặc đồ (Ví dụ: Mặc đồ Đầu Bếp thì tự động +2 Nông nghiệp)
    public ManagementBonusStats baseManagementBonus;

    [Header("--- KỸ NĂNG ĐI KÈM MẶC ĐỊNH ---")]
    public List<string> innateOutfitSkills;

    // ==========================================
    // MẠNG LƯỚI CÂY CÔNG NGHỆ (NON-LINEAR TECH TREE)
    // ==========================================
    [Header("--- CÂY CÔNG NGHỆ (MẠNG LƯỚI) ---")]
    public List<OutfitUpgradeNode> techTreeNodes;
}

// ==========================================
// CẤU TRÚC CHI PHÍ TÀI NGUYÊN
// ==========================================
[System.Serializable]
public struct ResourceCost
{
    public string resourceName;
    public int amount;
}

// ==========================================
// CẤU TRÚC "HỘP CHỨA" CHỈ SỐ QUẢN LÝ THƯỞNG
// (Tạo ra để Inspector gọn gàng, chứa đủ 7 chỉ số như bạn yêu cầu)
// ==========================================
[System.Serializable]
public struct ManagementBonusStats
{
    public int agriculture;      // Nông nghiệp
    public int industry;         // Công nghiệp
    public int commerce;         // Thương nghiệp
    public int militarySecurity; // An ninh Quân sự
    public int socioPolitics;    // Chính trị Xã hội
    public int wisdom;           // Trí tuệ
    public int activity;         // Hoạt động
}

// ==========================================
// CẤU TRÚC 1 Ô (NODE) TRONG CÂY CÔNG NGHỆ
// ==========================================
[System.Serializable]
public class OutfitUpgradeNode
{
    [Header("1. Định vị Ô (Logic Cây)")]
    public string nodeID = "Node_0";
    public List<string> requiredNodeIDs;
    public Vector2 uiPosition;
    public NodeType nodeType = NodeType.MinorStat;
    public bool isUnlocked = false;

    [Header("2. Yêu cầu Mở khóa (Nguyên liệu)")]
    public List<ResourceCost> unlockCosts;
    [Header("3. Thay đổi Ngoại hình (Mesh)")]
    public List<string> meshesToEnable;
    public List<string> meshesToDisable;

    [Header("4. Phần thưởng (Chỉ số & Kỹ năng)")]
    // node: Hộp chứa phần thưởng Chiến đấu (Có thể gõ số âm nếu mở nhầm ô nguyền rủa)
    public CombatStats bonusCombatStats;

    // node: Hộp chứa phần thưởng Quản lý (Đã gói đủ 7 chỉ số cực kỳ gọn gàng)
    public ManagementBonusStats bonusManagementStats;

    [Header("Phần thưởng Cơ chế Trận đấu (Battle Mechanics)")]
    [Tooltip("Cộng thêm AP Tối đa cho nhân vật")]
    public int bonusMaxAP;

    [Tooltip("Cộng thêm số lượng Item Card được rút (Mặc định 3, tối đa 6)")]
    public int bonusItemCardSlots;

    [Tooltip("Cộng thêm số lượng Skill Card được rút (Mặc định 3, tối đa 6)")]
    public int bonusSkillCardSlots;

    [Tooltip("Dị thường nhận được (Dành cho Ô To)")]
    public List<AnomalyData> newAnomalies;
}
using UnityEngine;
using System.Collections.Generic;

public enum CharacterRole { Player, Enemy, Boss, NPC_Army }

[CreateAssetMenu(fileName = "New Character Stat", menuName = "JRPG System/Character Data")]
public class CharacterStats : ScriptableObject
{
    [Header("--- THÔNG TIN CƠ BẢN ---")]
    public string characterName = "Tên nhân vật";
    public CharacterRole role = CharacterRole.Player;

    [Tooltip("Trang phục nhân vật đang mặc (Quyết định Hệ và Skill)")]
    public OutfitData currentOutfit;

    public Sprite portrait;
    public int level = 1;

    public int skillPointsPerLevel = 5;
    public int currentSkillPoints = 0;

    [Header("--- CHỈ SỐ CHIẾN ĐẤU ---")]
    [Space(10)]
    public CombatStats combatStats;

    [Header("--- CHI PHÍ & LƯỢNG NHẬN ĐƯỢC KHI NÂNG CẤP ---")]
    [Space(10)]
    public UpgradeSystem upgradeSystem;

    [Header("--- CHỈ SỐ QUẢN LÝ THÀNH TRÌ ---")]
    [Space(10)]
    public ManagementStats managementStats;

    [Header("--- KỸ NĂNG BẨM SINH (MỐC LEVEL) ---")]
    public List<LevelUpSkill> innateSkills;
}

// ==========================================
// CÁC STRUCT & CLASS DỮ LIỆU ĐỂ HIỂN THỊ LÊN INSPECTOR
// ==========================================

[System.Serializable]
// gọi hàm cộng điểm từ Visual Scripting hoặc file code khác dễ dàng và không bị lỗi.
public class Stat
{
    [Tooltip("Tài năng bẩm sinh của nhân vật (Khóa từ 0 đến 10)")]
    [Range(0, 10)]
    public int baseValue;

    [Tooltip("Điểm cộng thêm từ Sự kiện, Danh hiệu, Trang phục...")]
    public int bonusValue;

    public int Total => baseValue + bonusValue;

    // node: ĐÂY LÀ CHÌA KHÓA CHO SỰ KIỆN VÀ VISUAL SCRIPTING!
    // Bất kỳ sự kiện nào (+1 hay +5) cũng chỉ cần gọi hàm này và ném con số vào.
    public void AddBonus(int amount)
    {
        bonusValue += amount;
    }
}

[System.Serializable]
public struct CombatStats
{
    public int maxHP;
    public int maxMP;
    public int HIT;
    public int EVA;
    public int STR;
    public int CON;
    public int MAG;
    public int RES;
    public int AGI;
    public int LUK;
    public int MOV;
}

[System.Serializable]
public struct UpgradeSystem
{
    [Header("Chi phí nâng cấp (Cần bao nhiêu SP)")]
    public int costMaxHP;
    public int costMaxMP;
    public int costHIT;
    public int costEVA;
    public int costSTR;
    public int costCON;
    public int costMAG;
    public int costRES;
    public int costAGI;
    public int costLUK;
    public int costMOV;

    [Header("Lượng chỉ số nhận được (Khi ấn nâng cấp)")]
    public int hpGainPerUpgrade;
    public int mpGainPerUpgrade;
}
[System.Serializable]
public struct ManagementStats
{
    [Header("Kinh tế cơ bản")]
    public Stat agriculture;             // Nông nghiệp
    public Stat industry;                // Công nghiệp
    public Stat commerce;               // Thương nghiệp

    [Header("Quân sự & Chính trị")]
    public Stat militarySecurity;       // An ninh Quân sự
    public Stat socioPolitics;          // Chính trị Xã hội

    [Header("Khác")]
    public Stat wisdom;                 // Trí tuệ(Công nghệ + Ma thuật)
    public Stat activity;               // Hoạt động (Do thám, sự kiện)
}

[System.Serializable]
public struct LevelUpSkill
{
    public int unlockLevel;
    public string skillName;
}
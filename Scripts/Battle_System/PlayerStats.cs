using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("--- 1. CĂN CƯỚC CÔNG DÂN (DỮ LIỆU GỐC) ---")]
    public CharacterStats baseCharacterData;

    [Header("--- 2. CHỈ SỐ BATTLE HIỆN TẠI (ĐỘNG) ---")]
    public int maxHP;
    public int currentHP;

    public int maxMP;
    public int currentMP;

    public int maxAP = 3;
    public int currentAP;

    [Header("--- 3. KẾT NỐI UI (HIỂN THỊ CHỮ) ---")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI apText;
    public TextMeshProUGUI mpText;

    [Header("--- 3.1 KẾT NỐI UI (THANH MÀU) ---")]
    public Image hpBarFill; // Kéo HP_Bar_Fill ngoài Scene vào đây
    public Image mpBarFill; // Kéo MP_Bar_Fill ngoài Scene vào đây

    [Header("--- 4. KẾT NỐI HỆ THỐNG BÀI ---")]
    public HandManager handManager;




    void Start()
    {
        InitializeStats();
    }

    public void InitializeStats()
    {
        if (baseCharacterData != null)
        {
            maxHP = baseCharacterData.combatStats.maxHP;
            maxMP = baseCharacterData.combatStats.maxMP;
        }

        currentHP = maxHP;
        currentMP = maxMP;
        currentAP = maxAP;

        UpdateUI();
    }

    public void UpdateUI()
    {
        // 1. Cập nhật con số
        if (hpText != null) hpText.text = $"{currentHP}/{maxHP}";
        if (mpText != null) mpText.text = $"{currentMP}/{maxMP}";
        if (apText != null) apText.text = currentAP.ToString();

        // 2. Cập nhật độ dài của thanh màu (Từ 0.0 đến 1.0)
        if (hpBarFill != null) hpBarFill.fillAmount = (float)currentHP / maxHP;
        if (mpBarFill != null) mpBarFill.fillAmount = (float)currentMP / maxMP;
    }

    public bool CanAffordAP(int cost)
    {
        return currentAP >= cost;
    }

    public bool CanAffordMP(int cost)
    {
        return currentMP >= cost;
    }

    // ==========================================
    // ĐÃ SỬA LỖI Ở ĐÂY: Đổi thành RefreshHandVisuals()
    // ==========================================
    public void SpendAP(int cost)
    {
        currentAP -= cost;
        UpdateUI();
        if (handManager != null) handManager.RefreshHandVisuals();
    }

    public void SpendMP(int cost)
    {
        currentMP -= cost;
        UpdateUI();
        if (handManager != null) handManager.RefreshHandVisuals();
    }

    public void ResetAPForNewTurn()
    {
        currentAP = maxAP;
        UpdateUI();
        if (handManager != null) handManager.RefreshHandVisuals();
    }

    // ==========================================
    // NÚT BẤM ẢO DÀNH CHO BẠN TEST TRONG INSPECTOR
    // ==========================================
    [ContextMenu("TEST: Trừ 1 AP")]
    public void TestSpendAP()
    {
        SpendAP(1);
    }

    [ContextMenu("TEST: Trừ 10 MP")]
    public void TestSpendMP()
    {
        SpendMP(10);
    }
}
using UnityEngine;
using System.Collections.Generic;

public class AnomalyManager : MonoBehaviour
{
    [Header("--- KẾT NỐI HỆ THỐNG TRẬN ĐẤU ---")]
    public BattleManager battleManager;
    public HandManager handManager;

    [Header("--- DANH SÁCH DỊ THƯỜNG HIỆN TẠI ---")]
    public List<AnomalyData> activeBattleAnomalies = new List<AnomalyData>();

    public void InitializeAnomaliesOnStart()
    {
        if (battleManager != null && battleManager.statusUI != null)
        {
            battleManager.statusUI.LoadAnomalies(activeBattleAnomalies);
        }
    }

    public void ProcessAnomalies(AnomalyTriggerTime currentTime)
    {
        foreach (var anomaly in activeBattleAnomalies)
        {
            if (anomaly.triggerTime == currentTime)
            {
                // ==========================================
                // 💡 1. XỬ LÝ TỰ ĐỘNG ĐẺ TOKEN (SIÊU NHÀN)
                // ==========================================
                if (anomaly.tokensToGenerate != null && anomaly.tokensToGenerate.Count > 0)
                {
                    if (handManager != null && handManager.tokenManager != null)
                    {
                        handManager.tokenManager.AddTokens(anomaly.tokensToGenerate);
                        Debug.Log($"🔮 {anomaly.anomalyName} đã tự động đẻ ra {anomaly.tokensToGenerate.Count} Token!");
                    }
                }

                // ==========================================
                // ⚙️ 2. XỬ LÝ LOGIC CODE DÀNH CHO CÁC CA PHỨC TẠP
                // ==========================================
                if (!string.IsNullOrEmpty(anomaly.effectCode))
                {
                    ExecuteAnomalyEffect(anomaly.effectCode);
                }
            }
        }
    }

    private void ExecuteAnomalyEffect(string effectCode)
    {
        switch (effectCode)
        {
            // Các ca đẻ Token ta đã xóa hết, chỉ chừa lại các ca tác động chỉ số / cơ chế
            case "POISON_DAMAGE":
                Debug.Log("Đã dính độc: Trừ 10 HP!");
                break;

            case "SKIP_TURN":
                Debug.Log("Bị đóng băng: Trừ 50 AGI!");
                break;

            default:
                Debug.LogWarning("Chưa code logic cho Dị thường có mã: " + effectCode);
                break;
        }
    }
}
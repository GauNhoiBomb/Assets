using UnityEngine;
using System.Collections.Generic;

public enum AnomalyTriggerTime { TurnStart, TurnEnd, OnDrawCard, AlwaysOn }
public enum AnomalyType { Positive, Negative, Neutral }

[CreateAssetMenu(fileName = "New Anomaly", menuName = "JRPG System/Anomaly Data")]
public class AnomalyData : ScriptableObject
{
    [Header("--- THÔNG TIN DỊ THƯỜNG ---")]
    public string anomalyName = "Tên Dị thường";
    [TextArea(2, 3)]
    public string description = "Mô tả hiệu ứng...";
    public Sprite icon;
    public AnomalyType anomalyType = AnomalyType.Positive;

    [Header("--- LOGIC HOẠT ĐỘNG ---")]
    public AnomalyTriggerTime triggerTime = AnomalyTriggerTime.TurnStart;

    [Tooltip("Chỉ dùng cho các hiệu ứng phức tạp (Trừ HP, Khóa bài...). Bỏ trống nếu chỉ đẻ Token.")]
    public string effectCode;

    [Header("--- KẾT QUẢ KÍCH HOẠT (TỰ ĐỘNG) ---")]
    [Tooltip("Danh sách các Token sẽ tự động đẻ ra (Không cần viết code!)")]
    public List<TokenData> tokensToGenerate;
}
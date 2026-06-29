using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Thư viện để đổi chữ trên nút

public class WaitButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TurnTimelineManager timelineManager;
    private BattleManager bm;
    private TextMeshProUGUI buttonText;

    void Start()
    {
        // Tự động tìm các hệ thống
        timelineManager = FindAnyObjectByType<TurnTimelineManager>();
        bm = FindAnyObjectByType<BattleManager>();

        // Tìm dòng chữ (Text) gắn trên nút này
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        // 💡 Tự động đổi chữ trên nút tùy theo trạng thái của Lượt đi
        if (bm != null && buttonText != null)
        {
            if (bm.state == BattleState.NormalTurnPhase)
                buttonText.text = "WAIT"; // Lần đầu vào lượt là Wait
            else if (bm.state == BattleState.WaitTurnPhase)
                buttonText.text = "END";  // Lần 2 quay lại lượt biến thành End
        }
    }

    // Khi hơ chuột VÀO nút này
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (timelineManager != null && bm != null)
        {
            // Tùy theo Phase mà xuất hiện mũi tên khác nhau
            if (bm.state == BattleState.NormalTurnPhase)
            {
                timelineManager.ShowWaitPrediction(); // Trỏ xuống cuối Khay A
            }
            else if (bm.state == BattleState.WaitTurnPhase)
            {
                timelineManager.ShowEndPrediction();  // Trỏ thẳng sang Turn 2 (Khay B)
            }
        }
    }

    // Khi rút chuột RA
    public void OnPointerExit(PointerEventData eventData)
    {
        if (timelineManager != null)
        {
            timelineManager.HidePrediction();
        }
    }
}
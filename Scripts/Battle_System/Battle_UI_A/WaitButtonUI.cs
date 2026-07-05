using UnityEngine;
using UnityEngine.EventSystems;

public class WaitButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TurnTimelineManager timelineManager;
    private BattleManager bm;

    void Start()
    {
        // Tự động tìm các hệ thống
        timelineManager = FindAnyObjectByType<TurnTimelineManager>();
        bm = FindAnyObjectByType<BattleManager>();

        // 💡 ĐÃ XÓA: Hàm Update() và biến buttonText. 
        // note: Việc đổi chữ "WAIT" / "END" giờ đã được BattleUIManager_A quản lý triệt để khi đánh bài,
        // xóa ở đây để tránh xung đột 2 script cùng tranh nhau đổi chữ.
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
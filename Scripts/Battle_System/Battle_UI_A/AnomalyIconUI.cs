using UnityEngine;
using UnityEngine.EventSystems;

public class AnomalyIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AnomalyData myData;
    private AnomalyAndTurnStatusPanelUI myUI;

    // Nhận dữ liệu khi được sinh ra
    public void Setup(AnomalyData data, AnomalyAndTurnStatusPanelUI ui)
    {
        myData = data;
        myUI = ui;
    }

    // Khi hơ chuột VÀO
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myUI != null && myData != null)
        {
            myUI.ShowAnomalyTooltip(myData, GetComponent<RectTransform>().position);
        }
    }

    // Khi hơ chuột RA
    public void OnPointerExit(PointerEventData eventData)
    {
        if (myUI != null)
        {
            myUI.HideAnomalyTooltip();
        }
    }
}
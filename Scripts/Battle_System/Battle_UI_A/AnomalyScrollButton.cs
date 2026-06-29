using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnomalyScrollButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("--- KẾT NỐI HỆ THỐNG CUỘN ---")]
    public ScrollRect targetScrollRect;

    [Header("--- CÀI ĐẶT NÚT ---")]
    public bool isUpButton = true;
    public float scrollSpeed = 2f;

    private bool isPressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    void Update()
    {
        if (isPressed && targetScrollRect != null)
        {
            float step = scrollSpeed * Time.deltaTime;

            if (isUpButton)
                targetScrollRect.verticalNormalizedPosition += step;
            else
                targetScrollRect.verticalNormalizedPosition -= step;

            targetScrollRect.verticalNormalizedPosition = Mathf.Clamp01(targetScrollRect.verticalNormalizedPosition);
        }
    }
}
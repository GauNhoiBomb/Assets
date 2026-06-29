using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class AnomalyAndTurnStatusPanelUI : MonoBehaviour
{
    [Header("--- KẾT NỐI UI ---")]
    public TextMeshProUGUI turnText;
    public RectTransform anomalyPanel;
    public Transform anomalyContainer;
    public GameObject anomalyIconPrefab;

    [Header("--- HỆ THỐNG TOOLTIP ---")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    [Tooltip("Kéo trượt X, Y ngoài Inspector để căn chỉnh vị trí Tooltip vừa ý")]
    public Vector3 tooltipOffset = new Vector3(-200f, 0f, 0f);

    [Header("--- BẢNG MÀU DỊ THƯỜNG (DỰ PHÒNG) ---")]
    public Color positiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color negativeColor = new Color(0.8f, 0.1f, 0.1f, 1f);
    public Color neutralColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    [Header("--- KHUNG VIỀN ĐỒ HỌA (PNG) ---")]
    [Tooltip("Kéo ảnh khung PNG vào đây. Nếu để trống, hệ thống sẽ dùng Bảng màu ở trên.")]
    public Sprite positiveFrame; // Khung Tốt
    public Sprite negativeFrame; // Khung Xấu
    public Sprite neutralFrame;  // Khung Trung dung

    [Header("--- CÀI ĐẶT TRƯỢT PANEL ---")]
    public float slideSpeed = 10f;
    public float showX = 0f;
    public float hideX = 150f;

    private bool isShowing = true;
    private float targetX;

    void Start()
    {
        targetX = showX;
        HideAnomalyTooltip();
    }

    void Update()
    {
        if (anomalyPanel == null) return;
        Vector2 currentPos = anomalyPanel.anchoredPosition;
        currentPos.x = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * slideSpeed);
        anomalyPanel.anchoredPosition = currentPos;
    }

    public void OnToggleArrowClicked()
    {
        isShowing = !isShowing;
        targetX = isShowing ? showX : hideX;
        Transform arrow = transform.Find("Anomaly_Panel/Toggle_Arrow");
        if (arrow != null) arrow.localScale = new Vector3(isShowing ? 1 : -1, 1, 1);
    }

    public void UpdateTurnCounter(int currentTurn)
    {
        if (turnText != null) turnText.text = currentTurn.ToString();
    }

    public void LoadAnomalies(List<AnomalyData> anomalies)
    {
        foreach (Transform child in anomalyContainer) Destroy(child.gameObject);
        if (anomalies == null) return;

        foreach (var anomaly in anomalies)
        {
            GameObject newIcon = Instantiate(anomalyIconPrefab, anomalyContainer);

            Transform borderObj = newIcon.transform.Find("Border");
            if (borderObj != null)
            {
                Image borderImg = borderObj.GetComponent<Image>();
                if (borderImg != null)
                {
                    // 💡 MA THUẬT NẰM Ở ĐÂY: Ưu tiên dùng ảnh PNG nếu có, không có thì dùng Màu.
                    switch (anomaly.anomalyType)
                    {
                        case AnomalyType.Positive:
                            if (positiveFrame != null) { borderImg.sprite = positiveFrame; borderImg.color = Color.white; }
                            else { borderImg.color = positiveColor; }
                            break;
                        case AnomalyType.Negative:
                            if (negativeFrame != null) { borderImg.sprite = negativeFrame; borderImg.color = Color.white; }
                            else { borderImg.color = negativeColor; }
                            break;
                        case AnomalyType.Neutral:
                            if (neutralFrame != null) { borderImg.sprite = neutralFrame; borderImg.color = Color.white; }
                            else { borderImg.color = neutralColor; }
                            break;
                    }
                }
            }

            Transform iconObj = newIcon.transform.Find("Icon");
            if (iconObj != null)
            {
                Image iconImg = iconObj.GetComponent<Image>();
                if (iconImg != null && anomaly.icon != null) iconImg.sprite = anomaly.icon;
            }

            AnomalyIconUI iconScript = newIcon.GetComponent<AnomalyIconUI>();
            if (iconScript == null) iconScript = newIcon.gameObject.AddComponent<AnomalyIconUI>();
            iconScript.Setup(anomaly, this);
        }
    }

    public void ShowAnomalyTooltip(AnomalyData data, Vector3 iconPosition)
    {
        if (tooltipPanel == null || tooltipText == null) return;

        tooltipText.text = $"<b>{data.anomalyName}</b>\n<size=80%>{data.description}</size>";
        tooltipPanel.SetActive(true);

        tooltipPanel.transform.position = iconPosition + tooltipOffset;
    }

    public void HideAnomalyTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}
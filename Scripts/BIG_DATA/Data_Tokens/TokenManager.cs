using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TokenManager : MonoBehaviour
{
    [Header("--- KẾT NỐI UI ---")]
    public Transform tokenTray;
    public GameObject tokenPrefab;

    [Header("--- CÀI ĐẶT HOẠT ẢNH (ANIMATION) ---")]
    public int maxTokens = 20;
    public float spacing = 60f;     // Khoảng cách giữa các token (Pixel)
    public float slideSpeed = 12f;  // Tốc độ trượt vào

    public List<TokenData> activeTokens = new List<TokenData>();
    private List<RectTransform> tokenUIList = new List<RectTransform>();

    void Update()
    {
        // THUẬT TOÁN ANIMATION: Liên tục kéo các Token về đúng vị trí xếp hàng của nó
        for (int i = 0; i < tokenUIList.Count; i++)
        {
            if (tokenUIList[i] != null)
            {
                float targetX = i * spacing; // Vị trí đích (0, 60, 120, 180...)
                Vector2 currentPos = tokenUIList[i].anchoredPosition;

                // Vuốt mượt từ vị trí hiện tại tới đích
                tokenUIList[i].anchoredPosition = Vector2.Lerp(currentPos, new Vector2(targetX, 0), Time.deltaTime * slideSpeed);
            }
        }
    }

    public void AddTokens(List<TokenData> tokensToAdd)
    {
        if (tokensToAdd == null || tokensToAdd.Count == 0) return;

        foreach (TokenData t in tokensToAdd)
        {
            activeTokens.Add(t);

            // 1. Đẻ ra Token
            GameObject newTokenUI = Instantiate(tokenPrefab, tokenTray);
            RectTransform rect = newTokenUI.GetComponent<RectTransform>();

            // 2. MẸO ANIMATION: Đặt nó sinh ra ở tít bên phải màn hình để nó trượt vào!
            float startX = (tokenUIList.Count + 1) * spacing + 300f;
            rect.anchoredPosition = new Vector2(startX, 0);

            tokenUIList.Add(rect);

            // Gắn ảnh
            Image tokenIcon = newTokenUI.GetComponent<Image>();
            if (tokenIcon != null && t.tokenIcon != null) tokenIcon.sprite = t.tokenIcon;

            // 3. LOGIC ĐẨY TRÀN 20 TOKEN
            if (activeTokens.Count > maxTokens)
            {
                activeTokens.RemoveAt(0); // Xóa dữ liệu cũ nhất
                GameObject oldToken = tokenUIList[0].gameObject;
                tokenUIList.RemoveAt(0);  // Rút nó khỏi hàng chờ
                Destroy(oldToken);        // Tiêu diệt cục UI
            }
        }
    }

    public void ClearAllTokens()
    {
        activeTokens.Clear();
        tokenUIList.Clear();
        foreach (Transform child in tokenTray) Destroy(child.gameObject);
    }
}
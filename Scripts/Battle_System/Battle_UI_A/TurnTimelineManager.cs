using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq;
using TMPro;

public class TurnTimelineManager : MonoBehaviour
{
    [Header("--- KẾT NỐI UI ---")]
    public Transform avatarContainer;
    public GameObject avatarPrefab;
    public GameObject roundDividerPrefab;

    [Header("--- BẢNG MÀU AVATAR TIMELINE ---")]
    public Color playerColor = Color.green;
    public Color enemyColor = Color.red;
    public Color npcColor = Color.cyan;

    [Header("--- KHUNG VIỀN ĐỒ HỌA (PNG) ---")]
    public Sprite playerFrame;
    public Sprite enemyFrame;
    public Sprite npcFrame;

    [Header("--- MŨI TÊN DỰ BÁO (CHỮ U) ---")]
    public GameObject ghostSpacePrefab;
    public RectTransform predictionBracket;
    public float arrowLengthOffset = 20f;

    [Header("Cài đặt Chiều dài Mũi tên (Pixel)")]
    public bool useManualArrowWidth = true;
    public float widthInPlace = 60f;
    public float widthPerJump = 50f;

    [Header("Cài đặt Hoạt ảnh Bay (Animation)")]
    public float inPlaceJumpDistance = 40f;
    public float avatarDropDepth = -60f;

    [Header("Cài đặt Độ rộng Khe hở (Ghost Space)")]
    public float ghostWidthInPlace = 60f;
    public float ghostWidthJump = 30f;

    [Header("--- HỆ THỐNG DEBUG BẮT LỖI ---")]
    public bool showDebugLogs = true;

    private GameObject ghostInstance;
    private bool isAnimating = false;
    private Coroutine currentDropCoroutine;

    [Header("--- HỆ THỐNG CUỘN (SCROLL) ---")]
    public RectTransform timelineViewport;
    public float scrollSpeed = 600f;

    private bool isScrollingLeft = false;
    private bool isScrollingRight = false;
    private RectTransform contentRect;

    private BattleManager bm;

    private void LogTracker(string message)
    {
        if (showDebugLogs) Debug.Log($"<color=orange>[TIMELINE TRACKER]</color> {message}");
    }

    void Start()
    {
        bm = FindAnyObjectByType<BattleManager>();
        if (avatarContainer != null) contentRect = avatarContainer.GetComponent<RectTransform>();

        if (ghostSpacePrefab != null)
        {
            ghostInstance = Instantiate(ghostSpacePrefab, avatarContainer);
            LayoutElement ghostLE = ghostInstance.GetComponent<LayoutElement>();
            if (ghostLE == null) ghostLE = ghostInstance.AddComponent<LayoutElement>();

            ghostInstance.SetActive(true);
            ghostLE.ignoreLayout = false;
            ghostLE.preferredWidth = 0;
            ghostLE.minWidth = 0;
        }

        if (predictionBracket != null) predictionBracket.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Mouse.current != null)
        {
            float scrollVal = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scrollVal) > 0.1f) ScrollTimeline(scrollVal > 0 ? scrollSpeed * 0.1f : -scrollSpeed * 0.1f);
        }
        if (isScrollingLeft) ScrollTimeline(scrollSpeed * Time.deltaTime);
        if (isScrollingRight) ScrollTimeline(-scrollSpeed * Time.deltaTime);
    }

    void ScrollTimeline(float amount)
    {
        if (contentRect == null || timelineViewport == null) return;
        Vector2 currentPos = contentRect.anchoredPosition;
        float maxScrollX = Mathf.Max(0, contentRect.rect.width - timelineViewport.rect.width);
        currentPos.x += amount;
        currentPos.x = Mathf.Clamp(currentPos.x, -maxScrollX, 0);
        contentRect.anchoredPosition = currentPos;
    }

    private void ForceLayoutRefresh()
    {
        if (avatarContainer == null) return;
        HorizontalLayoutGroup hlg = avatarContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            hlg.enabled = false;
            hlg.enabled = true;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(avatarContainer.GetComponent<RectTransform>());
    }

    public void RebuildTimeline(BattleUnit currentUnit, List<BattleUnit> normalQ, List<BattleUnit> waitQ, List<BattleUnit> nextQ)
    {
        if (isAnimating)
        {
            LogTracker("RebuildTimeline bị từ chối vì đang có hoạt ảnh bay!");
            return;
        }

        LogTracker("Bắt đầu RebuildTimeline (Xóa cũ, vẽ mới).");
        HidePrediction();

        foreach (Transform child in avatarContainer)
        {
            if (ghostInstance != null && child.gameObject == ghostInstance) continue;
            if (predictionBracket != null && child == predictionBracket) continue;
            Destroy(child.gameObject);
        }

        List<BattleUnit> khayA = new List<BattleUnit>();
        if (currentUnit != null) khayA.Add(currentUnit);
        if (normalQ != null) khayA.AddRange(normalQ);
        if (waitQ != null) khayA.AddRange(waitQ);

        var activeAndNormal = khayA.Where(u => !waitQ.Contains(u)).OrderByDescending(u => u.currentAGI).ToList();
        var waitList = khayA.Where(u => waitQ.Contains(u)).ToList();

        List<BattleUnit> sortedKhayA = new List<BattleUnit>();
        sortedKhayA.AddRange(activeAndNormal);
        sortedKhayA.AddRange(waitList);

        if (sortedKhayA.Count > 0) CreateTurnIndicator("TURN 1");

        foreach (var u in sortedKhayA)
        {
            bool isCurrent = (currentUnit != null && u == currentUnit);
            CreateAvatar(u, isCurrent);
        }

        if (nextQ != null && nextQ.Count > 0)
        {
            CreateTurnIndicator("TURN 2");
            foreach (var u in nextQ) CreateAvatar(u, false);
        }

        Canvas.ForceUpdateCanvases();
        LogTracker("RebuildTimeline hoàn tất.");
    }

    void CreateTurnIndicator(string turnText)
    {
        if (roundDividerPrefab == null) return;
        GameObject indicator = Instantiate(roundDividerPrefab, avatarContainer);
        indicator.name = "TurnIndicator_Flag";
        TextMeshProUGUI txt = indicator.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.text = turnText;
    }

    void CreateAvatar(BattleUnit unit, bool isCurrentTurn)
    {
        GameObject newAva = Instantiate(avatarPrefab, avatarContainer);

        Transform borderObj = newAva.transform.Find("Border");
        Image borderImg = (borderObj != null) ? borderObj.GetComponent<Image>() : newAva.GetComponent<Image>();

        if (borderImg != null)
        {
            switch (unit.role)
            {
                case CharacterRole.Player:
                    if (playerFrame != null) { borderImg.sprite = playerFrame; borderImg.color = Color.white; }
                    else { borderImg.sprite = null; borderImg.color = playerColor; }
                    break;
                case CharacterRole.Enemy:
                case CharacterRole.Boss:
                    if (enemyFrame != null) { borderImg.sprite = enemyFrame; borderImg.color = Color.white; }
                    else { borderImg.sprite = null; borderImg.color = enemyColor; }
                    break;
                case CharacterRole.NPC_Army:
                    if (npcFrame != null) { borderImg.sprite = npcFrame; borderImg.color = Color.white; }
                    else { borderImg.sprite = null; borderImg.color = npcColor; }
                    break;
            }
        }

        Transform iconObj = newAva.transform.Find("Icon");
        if (iconObj != null)
        {
            Image iconImg = iconObj.GetComponent<Image>();
            if (iconImg != null)
            {
                if (unit.baseData != null && unit.baseData.portrait != null)
                {
                    iconImg.sprite = unit.baseData.portrait;
                    iconImg.color = Color.white;
                }
                else iconImg.color = Color.white;
            }
        }

        if (isCurrentTurn) newAva.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }

    public void ShowPrediction(int delayAGI)
    {
        if (bm == null || bm.currentActiveUnit == null || isAnimating) return;

        List<BattleUnit> mockKhayA = new List<BattleUnit> { bm.currentActiveUnit };
        if (bm.normalTurnQueue != null) mockKhayA.AddRange(bm.normalTurnQueue);
        if (bm.waitTurnQueue != null) mockKhayA.AddRange(bm.waitTurnQueue);

        int simulatedAGI = bm.currentActiveUnit.currentAGI - delayAGI;

        var activeAndNormal = mockKhayA.Where(u => !bm.waitTurnQueue.Contains(u))
            .OrderByDescending(u => u == bm.currentActiveUnit ? simulatedAGI : u.currentAGI)
            .ThenBy(u => u == bm.currentActiveUnit ? 1 : 0)
            .ToList();
        var waitList = mockKhayA.Where(u => bm.waitTurnQueue.Contains(u)).ToList();

        List<BattleUnit> sortedMock = new List<BattleUnit>();
        sortedMock.AddRange(activeAndNormal);
        sortedMock.AddRange(waitList);
        if (bm.nextRoundQueue != null) sortedMock.AddRange(bm.nextRoundQueue);

        int targetIndexT = sortedMock.IndexOf(bm.currentActiveUnit);
        DrawPredictionBracketTo(targetIndexT);
    }

    public void ShowWaitPrediction()
    {
        if (bm == null || bm.currentActiveUnit == null || isAnimating) return;

        List<BattleUnit> mockNormal = new List<BattleUnit>();
        if (bm.normalTurnQueue != null) mockNormal.AddRange(bm.normalTurnQueue);

        List<BattleUnit> mockWait = new List<BattleUnit>();
        if (bm.waitTurnQueue != null) mockWait.AddRange(bm.waitTurnQueue);
        mockWait.Add(bm.currentActiveUnit);

        mockWait = mockWait.OrderBy(unit => unit.currentAGI).ToList();

        List<BattleUnit> sortedMock = new List<BattleUnit>();
        sortedMock.AddRange(mockNormal);
        sortedMock.AddRange(mockWait);
        if (bm.nextRoundQueue != null) sortedMock.AddRange(bm.nextRoundQueue);

        int targetIndexT = sortedMock.IndexOf(bm.currentActiveUnit);
        DrawPredictionBracketTo(targetIndexT);
    }

    public void ShowEndPrediction()
    {
        if (bm == null || bm.currentActiveUnit == null || isAnimating) return;

        List<BattleUnit> mockNormal = new List<BattleUnit>();
        if (bm.normalTurnQueue != null) mockNormal.AddRange(bm.normalTurnQueue);

        List<BattleUnit> mockWait = new List<BattleUnit>();
        if (bm.waitTurnQueue != null) mockWait.AddRange(bm.waitTurnQueue);

        List<BattleUnit> mockNext = new List<BattleUnit>();
        if (bm.nextRoundQueue != null) mockNext.AddRange(bm.nextRoundQueue);

        mockNext.Add(bm.currentActiveUnit);

        List<BattleUnit> sortedMock = new List<BattleUnit>();
        sortedMock.AddRange(mockNormal);
        sortedMock.AddRange(mockWait);
        sortedMock.AddRange(mockNext);

        int targetIndexT = sortedMock.IndexOf(bm.currentActiveUnit);
        DrawPredictionBracketTo(targetIndexT);
    }

    private void DrawPredictionBracketTo(int targetIndexT)
    {
        List<Transform> validUI = new List<Transform>();
        RectTransform activeAvatar = null;

        for (int i = 0; i < avatarContainer.childCount; i++)
        {
            Transform child = avatarContainer.GetChild(i);
            if (child.name.Contains("TurnIndicator") || child.name.Contains("Ghost") || child == predictionBracket) continue;

            validUI.Add(child);
            if (child.localScale.x > 1.1f) activeAvatar = child.GetComponent<RectTransform>();
        }

        if (activeAvatar == null || validUI.Count == 0) return;
        int currentIndexC = validUI.IndexOf(activeAvatar);

        LayoutElement ghostLE = ghostInstance.GetComponent<LayoutElement>();
        if (ghostLE == null) ghostLE = ghostInstance.gameObject.AddComponent<LayoutElement>();

        float targetWidth = (targetIndexT != currentIndexC) ? ghostWidthJump : ghostWidthInPlace;
        ghostLE.preferredWidth = targetWidth;
        ghostLE.minWidth = targetWidth;

        if (targetIndexT != currentIndexC)
        {
            int siblingInsertIndex = validUI[Mathf.Clamp(targetIndexT, 0, validUI.Count - 1)].GetSiblingIndex();
            if (targetIndexT > currentIndexC) siblingInsertIndex++;
            ghostInstance.transform.SetSiblingIndex(siblingInsertIndex);
        }
        else
        {
            int siblingInsertIndex = validUI[currentIndexC].GetSiblingIndex() + 1;
            ghostInstance.transform.SetSiblingIndex(siblingInsertIndex);
        }

        ForceLayoutRefresh();

        float targetX = ghostInstance.GetComponent<RectTransform>().anchoredPosition.x;

        if (predictionBracket != null)
        {
            LayoutElement le = predictionBracket.GetComponent<LayoutElement>();
            if (le == null) le = predictionBracket.gameObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            predictionBracket.gameObject.SetActive(true);
            predictionBracket.SetAsFirstSibling();

            RectTransform activeRect = activeAvatar.GetComponent<RectTransform>();
            float avatarWidth = activeRect.sizeDelta.x;
            float distance = targetX - activeRect.anchoredPosition.x;
            predictionBracket.anchoredPosition = new Vector2(activeRect.anchoredPosition.x, predictionBracket.anchoredPosition.y);

            int jumpCount = Mathf.Abs(targetIndexT - currentIndexC);

            if (useManualArrowWidth)
            {
                float customWidth = widthInPlace;
                if (jumpCount > 0) customWidth += (jumpCount * widthPerJump);
                predictionBracket.localScale = new Vector3(distance >= 0 ? 1 : -1, 1, 1);
                predictionBracket.sizeDelta = new Vector2(customWidth, predictionBracket.sizeDelta.y);
            }
            else
            {
                if (jumpCount == 0)
                {
                    predictionBracket.localScale = new Vector3(1, 1, 1);
                    predictionBracket.sizeDelta = new Vector2(avatarWidth * 0.5f, predictionBracket.sizeDelta.y);
                }
                else
                {
                    predictionBracket.localScale = new Vector3(distance >= 0 ? 1 : -1, 1, 1);
                    predictionBracket.sizeDelta = new Vector2(Mathf.Abs(distance) + arrowLengthOffset, predictionBracket.sizeDelta.y);
                }
            }
        }
    }

    public void HidePrediction()
    {
        if (ghostInstance != null && !isAnimating)
        {
            LayoutElement ghostLE = ghostInstance.GetComponent<LayoutElement>();
            if (ghostLE != null)
            {
                ghostLE.preferredWidth = 0;
                ghostLE.minWidth = 0;
                ForceLayoutRefresh();
            }
        }

        if (predictionBracket != null && !isAnimating) predictionBracket.gameObject.SetActive(false);
    }

    public void AnimateAvatarDrop(int delayAGI, System.Action onComplete)
    {
        if (isAnimating)
        {
            LogTracker("CẢNH BÁO: Đang bay rồi, từ chối lệnh bay mới! Có thể bạn đã click đúp bài.");
            return;
        }

        if (currentDropCoroutine != null) StopCoroutine(currentDropCoroutine);
        currentDropCoroutine = StartCoroutine(DoAvatarDropRoutine(delayAGI, onComplete));
    }

    IEnumerator DoAvatarDropRoutine(int delayAGI, System.Action onComplete)
    {
        LogTracker(">> Bắt đầu quy trình bay Avatar...");
        isAnimating = true;

        if (predictionBracket != null) predictionBracket.gameObject.SetActive(false);

        List<BattleUnit> mockKhayA = new List<BattleUnit> { bm.currentActiveUnit };
        if (bm.normalTurnQueue != null) mockKhayA.AddRange(bm.normalTurnQueue);
        if (bm.waitTurnQueue != null) mockKhayA.AddRange(bm.waitTurnQueue);

        int simulatedAGI = bm.currentActiveUnit.currentAGI - delayAGI;

        var activeAndNormal = mockKhayA.Where(u => !bm.waitTurnQueue.Contains(u))
            .OrderByDescending(u => u == bm.currentActiveUnit ? simulatedAGI : u.currentAGI)
            .ThenBy(u => u == bm.currentActiveUnit ? 1 : 0)
            .ToList();
        var waitList = mockKhayA.Where(u => bm.waitTurnQueue.Contains(u)).ToList();

        List<BattleUnit> sortedMock = new List<BattleUnit>();
        sortedMock.AddRange(activeAndNormal);
        sortedMock.AddRange(waitList);
        if (bm.nextRoundQueue != null) sortedMock.AddRange(bm.nextRoundQueue);

        int targetIndexT = sortedMock.IndexOf(bm.currentActiveUnit);

        List<Transform> validUI = new List<Transform>();
        RectTransform activeAvatar = null;

        for (int i = 0; i < avatarContainer.childCount; i++)
        {
            Transform child = avatarContainer.GetChild(i);
            if (child.name.Contains("TurnIndicator") || child.name.Contains("Ghost") || child == predictionBracket) continue;

            validUI.Add(child);
            if (child.localScale.x > 1.1f) activeAvatar = child.GetComponent<RectTransform>();
        }

        if (activeAvatar == null)
        {
            LogTracker("LỖI NGHIÊM TRỌNG: activeAvatar bị NULL trước khi bay. Hủy luồng!");
            isAnimating = false;
            onComplete?.Invoke();
            yield break;
        }

        if (ghostInstance != null && bm != null)
        {
            int currentIndexC = validUI.IndexOf(activeAvatar);
            Vector2 startPos = activeAvatar.anchoredPosition;

            GameObject startGhost = Instantiate(ghostSpacePrefab, avatarContainer);
            startGhost.name = "Ghost_Start_Temp";
            LayoutElement startLE = startGhost.GetComponent<LayoutElement>();
            if (startLE == null) startLE = startGhost.AddComponent<LayoutElement>();

            startGhost.SetActive(true);
            startLE.ignoreLayout = false;
            startLE.preferredWidth = activeAvatar.rect.width;
            startLE.minWidth = activeAvatar.rect.width;
            startGhost.transform.SetSiblingIndex(activeAvatar.GetSiblingIndex());

            // 💡 CHÌA KHÓA VÀNG Ở ĐÂY NÀY BẠN ƠI:
            LayoutElement activeLE = activeAvatar.GetComponent<LayoutElement>();
            if (activeLE == null) activeLE = activeAvatar.gameObject.AddComponent<LayoutElement>();
            activeLE.ignoreLayout = true;

            activeAvatar.SetAsLastSibling();

            LayoutElement ghostLE = ghostInstance.GetComponent<LayoutElement>();
            if (ghostLE == null) ghostLE = ghostInstance.gameObject.AddComponent<LayoutElement>();

            float targetWidth = (targetIndexT != currentIndexC) ? ghostWidthJump : ghostWidthInPlace;
            ghostLE.preferredWidth = targetWidth;
            ghostLE.minWidth = targetWidth;

            if (targetIndexT != currentIndexC)
            {
                int siblingInsertIndex = validUI[Mathf.Clamp(targetIndexT, 0, validUI.Count - 1)].GetSiblingIndex();
                if (targetIndexT > currentIndexC) siblingInsertIndex++;
                ghostInstance.transform.SetSiblingIndex(siblingInsertIndex);
            }
            else
            {
                int siblingInsertIndex = validUI[currentIndexC].GetSiblingIndex() + 1;
                ghostInstance.transform.SetSiblingIndex(siblingInsertIndex);
            }

            ForceLayoutRefresh();
            yield return new WaitForEndOfFrame();

            Vector2 targetPos = Vector2.zero;
            if (targetIndexT != currentIndexC)
                targetPos = new Vector2(ghostInstance.GetComponent<RectTransform>().anchoredPosition.x, startPos.y);
            else
                targetPos = new Vector2(startPos.x + inPlaceJumpDistance, startPos.y);

            float totalDuration = 0.4f;
            float elapsed = 0f;
            float flyHeight = avatarDropDepth;

            LogTracker($"Bắt đầu vòng lặp Lerp Animation. Tọa độ đích: {targetPos.x}");

            while (elapsed < totalDuration)
            {
                if (activeAvatar == null || startGhost == null)
                {
                    LogTracker("LỖI CẮT NGANG: Avatar hoặc khoảng trống đã bị một hàm khác tiêu hủy trong lúc bay!");
                    break;
                }

                elapsed += Time.deltaTime;
                float progress = elapsed / totalDuration;
                Vector2 currentPos = Vector2.zero;

                if (progress < 0.2f)
                {
                    float p = progress / 0.2f;
                    currentPos = Vector2.Lerp(startPos, new Vector2(startPos.x, startPos.y + flyHeight), p);
                }
                else if (progress < 0.8f)
                {
                    float p = (progress - 0.2f) / 0.6f;
                    currentPos = Vector2.Lerp(new Vector2(startPos.x, startPos.y + flyHeight), new Vector2(targetPos.x, targetPos.y + flyHeight), p);
                }
                else
                {
                    float p = (progress - 0.8f) / 0.2f;
                    currentPos = Vector2.Lerp(new Vector2(targetPos.x, targetPos.y + flyHeight), targetPos, p);
                }

                activeAvatar.anchoredPosition = currentPos;
                yield return null;
            }

            if (startGhost != null) Destroy(startGhost);
            if (activeAvatar != null) Destroy(activeAvatar.gameObject);
        }

        if (ghostInstance != null)
        {
            LayoutElement ghostLE = ghostInstance.GetComponent<LayoutElement>();
            if (ghostLE != null) { ghostLE.preferredWidth = 0; ghostLE.minWidth = 0; }
        }

        LogTracker("<< Hoạt ảnh bay kết thúc, trả quyền điều khiển.");
        isAnimating = false;

        if (bm != null)
            RebuildTimeline(bm.currentActiveUnit, bm.normalTurnQueue, bm.waitTurnQueue, bm.nextRoundQueue);

        onComplete?.Invoke();
    }
}
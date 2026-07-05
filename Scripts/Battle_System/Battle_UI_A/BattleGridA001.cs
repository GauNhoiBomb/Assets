using UnityEngine;
using System.Collections.Generic;

public class BattleGridA001 : MonoBehaviour
{
    [Header("Cài đặt Lưới chung")]
    public GameObject cellPrefab;
    public int cellSize = 4;
    public int gapSize = 1;

    [Header("1. CỤM TRUNG TÂM (Người chơi)")]
    public bool showCenter = true;
    public int centerWidth = 5;
    public int centerLength = 5;

    [Header("2. CỤM PHÍA BẮC (Quái vật)")]
    public bool showNorth = true;
    public int northWidth = 5;
    public int northLength = 5;

    [Header("3. CỤM PHÍA NAM (Quái vật)")]
    public bool showSouth = true;
    public int southWidth = 5;
    public int southLength = 5;

    [Header("4. CỤM PHÍA ĐÔNG (Quái vật)")]
    public bool showEast = true;
    public int eastWidth = 5;
    public int eastLength = 5;

    [Header("5. CỤM PHÍA TÂY (Quái vật)")]
    public bool showWest = true;
    public int westWidth = 5;
    public int westLength = 5;

    [Header("Tùy chỉnh hiển thị & Layer")]
    public float surfaceOffset = 0.1f;
    public string playerGridLayer = "PlayerGridLayerA001";
    public string enemyGridLayer = "EnemyGridLayerA001";

    [Header("--- THIẾT LẬP VẬT LIỆU Ô LƯỚI (ART) ---")]
    public Material matWhite;
    public Material matRed;
    public Material matBlue;
    public Material matGreen;

    [Header("--- BỘ LỌC PHÁT HIỆN VẬT THỂ ---")]
    public LayerMask obstacleLayer;
    public LayerMask allyLayer;

    [Header("--- NHÂN VẬT ĐANG HÀNH ĐỘNG ---")]
    public Transform activePlayerUnit;
    public int playerMoveDistance = 3;

    public List<GameObject> activeCells = new List<GameObject>();
    [HideInInspector] public List<GameObject> validMoveCells = new List<GameObject>();

    private Vector3 lastPlayerPos;

    void Start()
    {
        GenerateCrossGrid();

        if (activePlayerUnit != null)
        {
            lastPlayerPos = activePlayerUnit.position;
        }

        RefreshGridTacticalColors();
        SetGridVisibility(false, false);
    }

    void Update()
    {
        if (activePlayerUnit != null)
        {
            float distMoved = Vector2.Distance(
                new Vector2(activePlayerUnit.position.x, activePlayerUnit.position.z),
                new Vector2(lastPlayerPos.x, lastPlayerPos.z)
            );

            if (distMoved > 1f)
            {
                lastPlayerPos = activePlayerUnit.position;
                RefreshGridTacticalColors();
            }
        }
    }

    void GenerateCrossGrid()
    {
        Vector3 origin = transform.position;
        if (showCenter) CreateCluster(centerWidth, centerLength, origin.x - (centerWidth / 2f * cellSize) + (cellSize / 2f), origin.z - (centerLength / 2f * cellSize) + (cellSize / 2f), playerGridLayer, "Center");
        if (showNorth) CreateCluster(northWidth, northLength, origin.x - (northWidth / 2f * cellSize) + (cellSize / 2f), origin.z + (centerLength / 2f * cellSize) + (gapSize * cellSize) + (cellSize / 2f), enemyGridLayer, "North");
        if (showSouth) CreateCluster(southWidth, southLength, origin.x - (southWidth / 2f * cellSize) + (cellSize / 2f), (origin.z - (centerLength / 2f * cellSize)) - (gapSize * cellSize) - (southLength * cellSize) + (cellSize / 2f), enemyGridLayer, "South");
        if (showEast) CreateCluster(eastWidth, eastLength, origin.x + (centerWidth / 2f * cellSize) + (gapSize * cellSize) + (cellSize / 2f), origin.z - (eastLength / 2f * cellSize) + (cellSize / 2f), enemyGridLayer, "East");
        if (showWest) CreateCluster(westWidth, westLength, origin.x - (centerWidth / 2f * cellSize) - (gapSize * cellSize) - (westWidth * cellSize) + (cellSize / 2f), origin.z - (westLength / 2f * cellSize) + (cellSize / 2f), enemyGridLayer, "West");
    }

    void CreateCluster(int width, int length, float startX, float startZ, string layerName, string clusterName)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                float worldX = startX + (x * cellSize);
                float worldZ = startZ + (z * cellSize);
                Vector3 cellPosition = new Vector3(worldX, transform.position.y + surfaceOffset, worldZ);
                GameObject newCell = Instantiate(cellPrefab, cellPosition, Quaternion.Euler(90f, 0f, 0f), transform);
                newCell.name = $"Cell_{clusterName}_{x}_{z}";
                SetLayerRecursively(newCell, LayerMask.NameToLayer(layerName));
                activeCells.Add(newCell);
            }
        }
    }

    public void RefreshGridTacticalColors()
    {
        if (activePlayerUnit == null)
        {
            validMoveCells.Clear();
            return;
        }

        int playerLayerIndex = LayerMask.NameToLayer(playerGridLayer);
        validMoveCells.Clear();

        List<GameObject> obstacleCells = new List<GameObject>();
        List<GameObject> allyCells = new List<GameObject>();
        GameObject startCell = null;

        // BƯỚC 1: QUÉT TÌM VẬT THỂ
        foreach (GameObject cell in activeCells)
        {
            Renderer cellRenderer = cell.GetComponentInChildren<Renderer>();
            if (cellRenderer == null) continue;

            cellRenderer.sharedMaterial = matWhite;
            Vector3 scanPos = cell.transform.position + new Vector3(0, 0.5f, 0);

            bool isActivePlayerHere = false;
            float distToPlayerX = Mathf.Abs(cell.transform.position.x - activePlayerUnit.position.x);
            float distToPlayerZ = Mathf.Abs(cell.transform.position.z - activePlayerUnit.position.z);
            if (distToPlayerX < 2f && distToPlayerZ < 2f) isActivePlayerHere = true;

            if (isActivePlayerHere)
            {
                startCell = cell;
                cellRenderer.sharedMaterial = matGreen;
                validMoveCells.Add(cell);
            }
            else if (Physics.CheckSphere(scanPos, 1.5f, allyLayer))
            {
                allyCells.Add(cell);
                cellRenderer.sharedMaterial = matBlue;
            }
            else if (Physics.CheckSphere(scanPos, 1.5f, obstacleLayer))
            {
                obstacleCells.Add(cell);
                cellRenderer.sharedMaterial = matRed;
            }
        }

        // BƯỚC 2: TÍNH TOÁN ĐƯỜNG ĐI XANH LÁ (CHO PHÉP XUYÊN QUA ĐỒNG ĐỘI)
        if (startCell != null)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            Dictionary<GameObject, int> distances = new Dictionary<GameObject, int>();

            queue.Enqueue(startCell);
            distances[startCell] = 0;

            while (queue.Count > 0)
            {
                GameObject current = queue.Dequeue();
                int currentDist = distances[current];

                if (currentDist >= playerMoveDistance) continue;

                foreach (GameObject neighbor in activeCells)
                {
                    if (neighbor.layer != playerLayerIndex) continue;

                    // 💡 SỬA ĐỔI QUAN TRỌNG: Chỉ chặn nếu là Đá Đỏ. Không chặn Đồng đội Xanh lam nữa.
                    if (obstacleCells.Contains(neighbor)) continue;

                    if (!distances.ContainsKey(neighbor))
                    {
                        float dist = Vector3.Distance(current.transform.position, neighbor.transform.position);
                        if (dist > (cellSize - 0.1f) && dist < (cellSize + 0.1f))
                        {
                            distances[neighbor] = currentDist + 1;

                            // CHÌA KHÓA: Vẫn đẩy ô có đồng đội vào hàng đợi để thuật toán loang tiếp sang ô phía sau
                            queue.Enqueue(neighbor);

                            // NHƯNG: CHỈ biến thành ô Xanh Lá (Cho phép Click đứng lại) nếu KHÔNG CÓ đồng đội
                            if (!allyCells.Contains(neighbor))
                            {
                                Renderer neighborRen = neighbor.GetComponentInChildren<Renderer>();
                                if (neighborRen != null) neighborRen.sharedMaterial = matGreen;

                                validMoveCells.Add(neighbor);
                            }
                        }
                    }
                }
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void SetGridVisibility(bool showPlayerGrid, bool showEnemyGrid)
    {
        foreach (Transform child in transform)
        {
            Renderer ren = child.GetComponentInChildren<Renderer>();
            if (ren != null)
            {
                if (child.gameObject.layer == LayerMask.NameToLayer("PlayerGridLayerA001"))
                    ren.enabled = showPlayerGrid;
                else if (child.gameObject.layer == LayerMask.NameToLayer("EnemyGridLayerA001"))
                    ren.enabled = showEnemyGrid;
            }
        }
    }
}
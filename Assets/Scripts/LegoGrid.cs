using UnityEngine;
using System.Collections.Generic;

// ============================================================
// LegoGrid.cs
// Sahnedeki grid sistemini yönetir.
// - Her hücrenin dünya pozisyonunu hesaplar
// - Hangi hücrenin dolu olduğunu takip eder
// - Snap (yapışma) noktalarını döner
// - İsometrik projeksiyon burada hesaplanır
// ============================================================

public class LegoGrid : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────
    public static LegoGrid Instance { get; private set; }

    // ── Inspector Ayarları ────────────────────────────────────

    [Header("Grid Boyutları")]
    [Tooltip("Grid'in toplam genişliği (stud sayısı)")]
    public int gridCols = 60;

    [Tooltip("Grid'in toplam derinliği (stud sayısı)")]
    public int gridRows = 60;

    [Tooltip("Kaç katman yüksekliğine kadar çıkılabilir")]
    public int maxLayers = 5;

    [Header("Stud (Hücre) Boyutu")]
    [Tooltip("Bir studu dünya koordinatına çeviren isometrik X offset (piksel)")]
    public float cellSizeX = 0.47f;    // İsometrik: bir stud sağa gidince kaç px sağa

    [Tooltip("Bir studu dünya koordinatına çeviren isometrik Y offset (piksel)")]
    public float cellSizeY = 0.14f;    // İsometrik: bir stud sağa gidince kaç px aşağı

    [Tooltip("Bir katman yukarı çıkınca kaç px yukarı gidilir")]
    public float layerHeight = 4.6f;  // İsometrik 'brick' yüksekliği

    [Header("Grid Gösterimi")]
    public bool showGridGizmos = true;
    public Color gizmoColor = new Color(0f, 0.8f, 1f, 0.3f);

    // ── Grid Verisi ───────────────────────────────────────────
    // Key: (col, row, layer) → Value: o hücreyi tutan LegoPiece
    // Parça birden fazla hücre kaplarsa hepsine yazılır
    private Dictionary<Vector3Int, LegoPiece> occupiedCells
        = new Dictionary<Vector3Int, LegoPiece>();

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Awake()
    {
        // Singleton kurulumu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ── Koordinat Dönüşümleri ─────────────────────────────────

    /// <summary>
    /// Grid hücresi (col, row, layer) → Dünya pozisyonu (Vector3)
    /// İsometrik formül:
    ///   worldX = (col - row) * cellSizeX * 0.5
    ///   worldY = (col + row) * cellSizeY * 0.5 + layer * layerHeight
    /// </summary>
    public Vector3 GridToWorld(int col, int row, int layer = 0)
    {
        float worldX = (col - row) * cellSizeX * 0.5f;
        float worldY = (col + row) * cellSizeY * 0.5f + layer * layerHeight;
        // Z: Unity 2D'de sorting order için kullanacağız, world z = 0
        return new Vector3(worldX, worldY, 0f) + transform.position;
    }

    public Vector3 GridToWorld(Vector3Int cell)
        => GridToWorld(cell.x, cell.y, cell.z);

    /// <summary>
    /// Dünya pozisyonu → En yakın grid hücresi
    /// Ters isometrik projeksiyon ile hesaplanır.
    /// layer parametresi: hangi katmanda aranacağını belirtir
    /// </summary>
    public Vector3Int WorldToGrid(Vector3 worldPos, int layer = 0)
    {
        // transform.position'u çıkar: lokal koordinata geç
        Vector3 local = worldPos - transform.position;

        // İsometrik ters dönüşüm:
        // col = (x/cellSizeX + y/cellSizeY) 
        // row = (y/cellSizeY - x/cellSizeX)
        float col = (local.x / cellSizeX + local.y / cellSizeY);
        float row = (local.y / cellSizeY - local.x / cellSizeX);

        return new Vector3Int(
            Mathf.RoundToInt(col),
            Mathf.RoundToInt(row),
            layer
        );
    }

    // ── Doluluk Kontrolü ──────────────────────────────────────

    /// <summary>
    /// Belirtilen hücrenin dolu olup olmadığını kontrol eder
    /// </summary>
    public bool IsCellOccupied(int col, int row, int layer)
    {
        return occupiedCells.ContainsKey(new Vector3Int(col, row, layer));
    }

    /// <summary>
    /// Bir parçanın belirtilen pozisyona yerleşip yerleşemeyeceğini kontrol eder.
    /// Parçanın kapladığı tüm hücreler boş VE grid sınırları içinde olmalı.
    /// </summary>
    public bool CanPlacePiece(LegoPieceData data, int col, int row, int layer)
    {
        for (int c = 0; c < data.gridWidth; c++)
        {
            for (int r = 0; r < data.gridHeight; r++)
            {
                int checkCol = col + c;
                int checkRow = row + r;

                // Sınır kontrolü
                if (checkCol < 0 || checkCol >= gridCols ||
                    checkRow < 0 || checkRow >= gridRows ||
                    layer < 0 || layer >= maxLayers)
                    return false;

                // Doluluk kontrolü
                if (IsCellOccupied(checkCol, checkRow, layer))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Mouse pozisyonunun altındaki en uygun "üst" katmanı bulur.
    /// Yani: o col/row'da en üstte hangi katman dolu, bir üstüne öner.
    /// </summary>
    public int GetTopLayer(int col, int row)
    {
        for (int layer = maxLayers - 1; layer >= 0; layer--)
        {
            if (IsCellOccupied(col, row, layer))
            {
                Debug.Log($"[TOP] ({col},{row}) -> layer {layer} dolu, ustune cik → {layer + 1}");
                return layer + 1; // üst üste oturma: dolu katmanın bir üstü
            }
        }
        return 0; // hiç doluyoksa zemine koy
    }

    // ── Parça Kayıt / Silme ───────────────────────────────────

    /// <summary>
    /// Parçanın kapladığı tüm hücreleri dolu olarak işaretler
    /// </summary>
    public void RegisterPiece(LegoPiece piece, int col, int row, int layer)
    {
        int gw = piece.EffectiveGridWidth;
        int gh = piece.EffectiveGridHeight;
        Debug.Log($"[REG] {piece.gameObject.name} → ({col},{row},layer={layer}) size={gw}x{gh}");
        for (int c = 0; c < gw; c++)
            for (int r = 0; r < gh; r++)
                occupiedCells[new Vector3Int(col + c, row + r, layer)] = piece;
        piece.gridPosition = new Vector3Int(col, row, layer);
    }

    /// <summary>
    /// Parçayı grid'den kaldırır (sürükleme başladığında çağrılır)
    /// </summary>
    public void UnregisterPiece(LegoPiece piece)
    {
        // Bu parçayı işaret eden TÜM hücreleri sil
        var keysToRemove = new List<Vector3Int>();
        foreach (var kvp in occupiedCells)
        {
            if (kvp.Value == piece)
                keysToRemove.Add(kvp.Key);
        }
        foreach (var key in keysToRemove)
            occupiedCells.Remove(key);

        Debug.Log($"[UNREG] {piece.gameObject.name} → {keysToRemove.Count} hücre silindi");

    }

    // ── Snap Noktası Hesaplama ────────────────────────────────

    /// <summary>
    /// Verilen dünya pozisyonu için en yakın geçerli snap noktasını döner.
    /// Geri dönüş: (snapWorldPos, gridCell, isValid)
    /// </summary>
    public (Vector3 worldPos, Vector3Int cell, bool isValid)
        GetSnapPoint(Vector3 worldPos, LegoPieceData data, LegoPiece piece = null)
    {
        // Etkin boyutlar (rotation'a göre)
        int gw = piece != null ? piece.EffectiveGridWidth  : data.gridWidth;
        int gh = piece != null ? piece.EffectiveGridHeight : data.gridHeight;
        // Parçanın merkez offset'ini hesapla (gridWidth/Height bilgisinden)
        float midCol = (data.gridWidth - 1) * 0.5f;
        float midRow = (data.gridHeight - 1) * 0.5f;
        float offsetX = (midCol - midRow) * cellSizeX * 0.5f;
        float offsetY = (midCol + midRow) * cellSizeY * 0.5f;
        Vector3 centerOffset = new Vector3(offsetX, offsetY, 0f);

        // Verilen worldPos parçanın merkezi varsayılır → sol-alt köşeyi bul
        Vector3 cornerWorld = worldPos - centerOffset;

        // Sol-alt köşe → grid hücresi
        Vector3Int baseCell = WorldToGrid(cornerWorld, 0);
        int col = baseCell.x;
        int row = baseCell.y;

        // Top layer
        int layer = GetTopLayer(col, row);

        // Geçerli mi?
        //bool valid = CanPlacePiece(data, col, row, layer);
        // CanPlacePiece için de effective boyut lazım, yeni overload
        bool valid = CanPlacePieceSized(gw, gh, col, row, layer);

        // Snap pos = parçanın MERKEZİ olması gereken yer
        Vector3 snapPos = GridToWorld(col, row, layer) + centerOffset;
        
        Debug.Log($"[SNAP] worldPos={worldPos} → corner={cornerWorld} | " +
                  $"baseCell=({col},{row}) → topLayer={layer} | " +
                  $"size={gw}x{gh} | valid={valid} | dolu hücre sayısı={occupiedCells.Count}");
        

        return (snapPos, new Vector3Int(col, row, layer), valid);
    }

    // Yeni helper: data yerine direkt boyut alır
    public bool CanPlacePieceSized(int gw, int gh, int col, int row, int layer)
    {
        for (int c = 0; c < gw; c++)
        for (int r = 0; r < gh; r++)
        {
            int cc = col + c, rr = row + r;
            if (cc < 0 || cc >= gridCols || rr < 0 || rr >= gridRows ||
                layer < 0 || layer >= maxLayers) return false;
            if (IsCellOccupied(cc, rr, layer)) return false;
        }
        return true;
    }

    // ── Gizmos (Editor'da Grid Görselleştirme) ────────────────
    private void OnDrawGizmos()
    {
        if (!showGridGizmos) return;

        Gizmos.color = gizmoColor;

        // Zemin katmanını çiz
        for (int c = 0; c <= gridCols; c++)
        {
            for (int r = 0; r <= gridRows; r++)
            {
                Vector3 pos = GridToWorld(c, r, 0);
                Gizmos.DrawSphere(pos, 0.03f);
            }
        }
        // Dolu hücreleri kırmızı kare olarak göster
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        foreach (var kvp in occupiedCells)
        {
            Vector3Int cell = kvp.Key;
            Vector3 pos = GridToWorld(cell.x, cell.y, cell.z);
            // İsometric "diamond" şekli — 4 köşe çiz
            Vector3 right = new Vector3(cellSizeX * 0.5f, cellSizeY * 0.5f, 0);
            Vector3 up    = new Vector3(-cellSizeX * 0.5f, cellSizeY * 0.5f, 0);
            Gizmos.DrawLine(pos, pos + right);
            Gizmos.DrawLine(pos + right, pos + right + up);
            Gizmos.DrawLine(pos + right + up, pos + up);
            Gizmos.DrawLine(pos + up, pos);
        }
    }
}

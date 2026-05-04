using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// ============================================================
// LegoBuilder.cs
// Sahne yöneticisi.
//
// Ne yapar:
//   - Oyun başında belirlenen sayıda parçayı (default: 7 küçük + 4 orta + 4 büyük)
//     sahnenin "scatter" alanına rastgele dağıtır.
//   - Yerleştirilemeyen parçaları (geçersiz snap) sahnedeki başlangıç noktasına
//     veya yeni rastgele bir konuma geri gönderir.
//   - SpriteExporter'ı tetikler.
//
// Spawn paneli YOK. Tüm parçalar zaten sahnede duruyor.
// ============================================================

public class LegoBuilder : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────
    public static LegoBuilder Instance { get; private set; }

    // ── Parça Sayısı Ayarları ─────────────────────────────────
    [System.Serializable]
    public class PieceSpawnConfig
    {
        [Tooltip("Hangi parça tipinden olacak")]
        public LegoPieceData pieceData;

        [Tooltip("Bu tipten kaç adet sahneye konacak")]
        public int count;
    }

    [Header("Spawn Listesi")]
    [Tooltip("Her parça tipinden kaç adet üretilecek. Default: 7 küçük + 4 orta + 4 büyük")]
    public List<PieceSpawnConfig> spawnConfigs = new List<PieceSpawnConfig>();

    [Header("Parça Prefab'ı")]
    [Tooltip("Tüm parçalar bu prefab'dan türetilir; LegoPiece bileşeni içermeli")]
    public GameObject piecePrefab;

    // ── Scatter (Dağıtım) Alanı ───────────────────────────────
    [Header("Dağıtım Alanı (Scatter Zone)")]
    [Tooltip("Parçaların rastgele dağıtılacağı alanın merkezi (dünya koordinatı)")]
    public Transform scatterCenter;

    [Tooltip("Dağıtım alanının boyutu (genişlik x yükseklik, dünya birimi)")]
    public Vector2 scatterSize = new Vector2(20f, 12f);

    [Tooltip("Parçaların birbirinden minimum uzaklığı (üst üste binmesin)")]
    public float minPieceDistance = 2.5f;

    [Tooltip("Rastgele dağıtım için seed. -1 = her oyunda farklı")]
    public int randomSeed = -1;

    [Header("Sahne Başlangıcı")]
    [Tooltip("Oyun başında otomatik dağıt")]
    public bool spawnOnStart = true;

    // ── Çalışma Zamanı Verisi ─────────────────────────────────
    // Sahnede var olan tüm parçalar (yerleştirilmiş + scatter'da duran)
    private List<LegoPiece> allPieces = new List<LegoPiece>();

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        AudioManager.Instance.PlayMusic("LegoBuilding");
        if (spawnOnStart)
            SpawnAllPieces();
    }

    // ── Parça Dağıtımı ────────────────────────────────────────

    /// <summary>
    /// spawnConfigs listesindeki tüm parçaları üretir ve scatter alanına dağıtır.
    /// </summary>
    public void SpawnAllPieces()
    {
        // Random seed ayarla (deterministic test için)
        if (randomSeed >= 0)
            Random.InitState(randomSeed);

        // Daha önce yerleştirilmiş pozisyonları tut → minPieceDistance kontrolü için
        List<Vector3> placedPositions = new List<Vector3>();

        foreach (var config in spawnConfigs)
        {
            if (config.pieceData == null) continue;

            for (int i = 0; i < config.count; i++)
            {
                // Rastgele bir scatter pozisyonu bul
                Vector3 spawnPos = FindRandomScatterPosition(placedPositions);
                placedPositions.Add(spawnPos);

                // Parçayı oluştur
                SpawnPiece(config.pieceData, spawnPos);
            }
        }

        Debug.Log($"LegoBuilder: {allPieces.Count} parça sahneye dağıtıldı.");
    }

    /// <summary>
    /// Tek bir parça oluşturur ve listeye ekler.
    /// </summary>
    private LegoPiece SpawnPiece(LegoPieceData data, Vector3 position)
    {
        GameObject obj = Instantiate(piecePrefab, position, Quaternion.identity);
        obj.name = $"LegoPiece_{data.pieceName}_{allPieces.Count}";

        LegoPiece piece = obj.GetComponent<LegoPiece>();
        piece.Initialize(data);

        // Scatter pozisyonunu "ev" olarak kaydet → geçersiz drop'ta buraya döner
        piece.SetScatterHome(position);

        allPieces.Add(piece);
        return piece;
    }

    // ── Scatter Pozisyon Bulucu ───────────────────────────────

    /// <summary>
    /// Grid hücrelerinden rastgele bir tanesini seçer ve dünya pozisyonunu döner.
    /// Mevcut parçalardan minPieceDistance uzakta olanları tercih eder.
    /// </summary>
    private Vector3 FindRandomScatterPosition(List<Vector3> existingPositions)
    {
        var grid = LegoGrid.Instance;
        if (grid == null)
        {
            Debug.LogWarning("[LegoBuilder] LegoGrid bulunamadı, fallback pozisyon kullanılıyor.");
            return Vector3.zero;
        }

        // Ana kameranın görüş alanını hesapla (orthographic varsayılır)
        Camera mainCam = Camera.main;
        float camPadding = 1f; // Kenara çok yakın olmasın diye iç boşluk
        float camMinX = 0, camMaxX = 0, camMinY = 0, camMaxY = 0;
        bool useCamFilter = false;

        if (mainCam != null && mainCam.orthographic)
        {
            float halfH = mainCam.orthographicSize - camPadding;
            float halfW = halfH * mainCam.aspect;
            Vector3 cp = mainCam.transform.position;
            camMinX = cp.x - halfW;
            camMaxX = cp.x + halfW;
            camMinY = cp.y - halfH;
            camMaxY = cp.y + halfH;
            useCamFilter = true;
        }

        const int maxAttempts = 100;
        Vector3 lastTry = Vector3.zero;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int col = Random.Range(0, grid.gridCols);
            int row = Random.Range(0, grid.gridRows);

            Vector3 candidate = grid.GridToWorld(col, row, 0);
            lastTry = candidate;

            // Kamera dışındaysa atla
            if (useCamFilter)
            {
                if (candidate.x < camMinX || candidate.x > camMaxX ||
                    candidate.y < camMinY || candidate.y > camMaxY)
                    continue;
            }

            // Mesafe kontrolü
            bool tooClose = false;
            foreach (var existing in existingPositions)
            {
                if (Vector3.Distance(candidate, existing) < minPieceDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose) return candidate;
        }

        return lastTry;
    }
    // ── Geçersiz Drop İade ────────────────────────────────────

    /// <summary>
    /// LegoPiece, geçersiz bir snap noktasına bırakıldığında bunu çağırır.
    /// Parça scatter "evine" geri döner — silinmez, sahnede kalır.
    /// </summary>
    public void ReturnToScatter(LegoPiece piece)
    {
        // Parçanın kayıtlı scatter home'una dön
        piece.transform.position = piece.ScatterHome;

        // Sorting order'ı normalize et (drag sırasında 999'a çıkmıştı)
        piece.ResetSortingOrder();
    }

    // ── Export Tetikleyici ────────────────────────────────────

    /// <summary>
    /// Yerleştirilmiş parçaları PNG olarak export eder.
    /// Sadece grid'e yerleşmiş olanlar export'a dahil edilir,
    /// scatter'da duranlar dahil edilmez.
    /// </summary>
    public void ExportModel()
    {
        SpriteExporter exporter = FindObjectOfType<SpriteExporter>(true);
        if (exporter == null)
        {
            Debug.LogWarning("LegoBuilder: SpriteExporter bulunamadı!");
            return;
        }

        // ── Export öncesi: dragged olmayan parçaları gizle ──
        List<LegoPiece> hiddenPieces = new List<LegoPiece>();
        foreach (var piece in allPieces)
        {
            if (!piece.WasEverDragged && piece.BodyRenderer != null)
            {
                piece.BodyRenderer.enabled = false;
                hiddenPieces.Add(piece);
                // Face child renderer da kapat
                var face = piece.transform.Find("Face");
                if (face != null)
                {
                    var faceR = face.GetComponent<SpriteRenderer>();
                    if (faceR != null) faceR.enabled = false;
                }
            }
        }

        // Yerleştirilmiş tüm parçaları export et (görünür olanlar render edilecek)
        List<LegoPiece> toExport = new List<LegoPiece>();
        foreach (var piece in allPieces)
        {
            if (piece.IsPlaced) toExport.Add(piece);
        }

        if (toExport.Count > 0)
        {
            Debug.Log($"[LegoBuilder] {toExport.Count} parça export ediliyor.");
            exporter.Export(toExport);
        }
        else
        {
            Debug.LogWarning("LegoBuilder: Yerleştirilmiş parça yok, export iptal.");
        }

        // ── Export sonrası: gizlenen parçaları geri göster ──
        foreach (var piece in hiddenPieces)
        {
            if (piece.BodyRenderer != null)
                piece.BodyRenderer.enabled = true;
            var face = piece.transform.Find("Face");
            if (face != null)
            {
                var faceR = face.GetComponent<SpriteRenderer>();
                if (faceR != null) faceR.enabled = true;
            }
        }
    }

    /// <summary>
    /// Parçanın sprite bounds'u kameranın görüş alanı içinde mi?
    /// </summary>
    private bool IsInsideCamera(LegoPiece piece, Camera cam)
    {
        if (piece.BodyRenderer == null || piece.BodyRenderer.sprite == null) 
            return false;

        Bounds b = piece.BodyRenderer.bounds;

        // Kameranın orthographic dikdörtgeni
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;
        float minX = c.x - halfW, maxX = c.x + halfW;
        float minY = c.y - halfH, maxY = c.y + halfH;

        // Bounds tamamen dışarıda mı?
        if (b.max.x < minX || b.min.x > maxX) return false;
        if (b.max.y < minY || b.min.y > maxY) return false;
        return true;
    }

    // ── Yardımcı: Scatter alanını editor'da göster ────────────
    private void OnDrawGizmosSelected()
    {
        Vector3 center = scatterCenter != null ? scatterCenter.position : transform.position;
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        Gizmos.DrawWireCube(center, new Vector3(scatterSize.x, scatterSize.y, 0.1f));
    }

    // ── Public Getter'lar ─────────────────────────────────────
    public List<LegoPiece> GetAllPieces() => allPieces;


    /// <summary>
    /// Önce export et, sonra GameScene'e geç.
    /// </summary>
    public void ExportAndGoToGame()
    {
        ExportModel();
        SceneManager.LoadScene("GameScene");
    }
}

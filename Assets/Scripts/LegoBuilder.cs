using UnityEngine;
using System.Collections.Generic;

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
    /// Scatter alanı içinde, mevcut parçalardan minPieceDistance uzakta
    /// rastgele bir pozisyon döner. Bulamazsa son denemeyi döner.
    /// </summary>
    private Vector3 FindRandomScatterPosition(List<Vector3> existingPositions)
    {
        Vector3 center = scatterCenter != null ? scatterCenter.position : Vector3.zero;
        const int maxAttempts = 30;

        Vector3 lastTry = center;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Scatter alanı içinde rastgele nokta
            float x = Random.Range(-scatterSize.x * 0.5f, scatterSize.x * 0.5f);
            float y = Random.Range(-scatterSize.y * 0.5f, scatterSize.y * 0.5f);
            Vector3 candidate = center + new Vector3(x, y, 0f);
            lastTry = candidate;

            // Mevcut parçalarla mesafe kontrolü
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

        // 30 denemede uygun yer bulunamadıysa son denemeyi kullan
        // (alanı büyütmek veya parça sayısını azaltmak gerekebilir)
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
        SpriteExporter exporter = FindObjectOfType<SpriteExporter>();
        if (exporter == null)
        {
            Debug.LogWarning("LegoBuilder: SpriteExporter bulunamadı!");
            return;
        }

        // Sadece grid'e yerleştirilmiş parçaları topla
        List<LegoPiece> placed = new List<LegoPiece>();
        foreach (var piece in allPieces)
        {
            if (piece.IsPlaced) placed.Add(piece);
        }

        if (placed.Count == 0)
        {
            Debug.LogWarning("LegoBuilder: Henüz yerleştirilmiş parça yok, export iptal.");
            return;
        }

        exporter.Export(placed);
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
}

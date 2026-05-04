using UnityEngine;

// ============================================================
// LegoPiece.cs
// Sahnedeki her lego parçasının bileşeni.
// Sorumlulukları:
//   1. Mouse ile sürükle-bırak
//   2. F tuşuyla yüzü flip et (sağa/sola bak)
//   3. Grid'e snap ile yerleşme
//   4. Sorting order (isometrik derinlik sıralaması)
// ============================================================

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class LegoPiece : MonoBehaviour
{
    // Bu parça en az bir kere kullanıcı tarafından sürüklendi mi?
    private bool wasEverDragged = false;
    public bool WasEverDragged => wasEverDragged;

    [Header("Drag Ayarı")]
    [Tooltip("Sürüklerken parça mouse'un kaç birim üstünde dursun (ghost ile çakışmasın)")]
    public float dragLiftAmount = 1.5f;
    // ── Veri ──────────────────────────────────────────────────
    [HideInInspector] public LegoPieceData data;

    // Grid'deki konumu (col, row, layer) — LegoGrid tarafından set edilir
    [HideInInspector] public Vector3Int gridPosition;

    // ── Sprite Referansları ───────────────────────────────────
    private SpriteRenderer bodyRenderer;    // Gövde sprite renderer'ı
    private SpriteRenderer faceRenderer;    // Yüz sprite renderer'ı (child obje)

    // ── Durum Değişkenleri ────────────────────────────────────
    private bool isDragging = false;        // Şu an sürükleniyor mu?
    private bool isPlaced = false;          // Grid'e yerleştirildi mi?
    private bool isFacingRight = true;      // Yüz sağa mı bakıyor?
    // Parça 90° döndürülmüş mü?
    private bool isRotated = false;
    public bool IsRotated => isRotated;

    // Rotation'a göre etkin grid boyutları
    public int EffectiveGridWidth  => isRotated ? data.gridHeight : data.gridWidth;
    public int EffectiveGridHeight => isRotated ? data.gridWidth  : data.gridHeight;

    // Sürükleme sırasında mouse offset'i (tıklama noktasının pivot'tan farkı)
    private Vector3 dragOffset;

    // Parça seçili mi? (flip tuşu sadece seçili parçaya uygulanır)
    private bool isSelected = false;

    // Rotation'a göre etkin sprite offset
    public Vector2 EffectiveSpriteOffset => 
        isRotated ? data.spriteOffsetRotated : data.spriteOffset;

    // ── Scatter Home ──────────────────────────────────────────
    // Parçanın oyun başında dağıtıldığı "ev" pozisyonu.
    // Geçersiz drop olduğunda buraya geri döner.
    private Vector3 scatterHome;
    public Vector3 ScatterHome => scatterHome;

    public void SetScatterHome(Vector3 position) => scatterHome = position;

    // Drag öncesi orijinal sorting order — ResetSortingOrder ile geri dönülür
    private int originalBodyOrder;
    private int originalFaceOrder;

    // ── Placement Guide ───────────────────────────────────────
    // PlacementGuide script'i bu parça sürüklenirken güncellenir
    private PlacementGuide placementGuide;

    // ── Sorting ───────────────────────────────────────────────
    // İsometrik sıralamada col+row+layer büyük olan üstte çizilir
    private int baseSortingOrder = 0;

    // ── Initialization ────────────────────────────────────────
    private void Awake()
    {
        bodyRenderer = GetComponent<SpriteRenderer>();

        // Yüz renderer'ı child objesinde arar
        // Hierarchy: LegoPiece > FaceObject(SpriteRenderer)
        Transform faceTransform = transform.Find("Face");
        if (faceTransform != null)
            faceRenderer = faceTransform.GetComponent<SpriteRenderer>();

        // PlacementGuide singleton'ını bul
        placementGuide = FindObjectOfType<PlacementGuide>();
    }

    /// <summary>
    /// Parçayı veriyle başlatır. LegoBuilder.SpawnPiece() tarafından çağrılır.
    /// </summary>
    public void Initialize(LegoPieceData pieceData)
    {
        data = pieceData;

        // Body sprite
        bodyRenderer.sprite = data.pieceSprite;
        bodyRenderer.color = data.pieceColor;

        // Yüz sprite — başlangıçta sağa bakıyor
        if (faceRenderer != null && data.faceSpriteRight != null)
        {
            faceRenderer.sprite = data.faceSpriteRight;
            faceRenderer.flipX = false;
        }

        // Collider boyutunu sprite'a göre otomatik ayarla (BoxCollider2D ise)
        var box = GetComponent<BoxCollider2D>();
        if (box != null && bodyRenderer.sprite != null)
        {
            box.size = bodyRenderer.sprite.bounds.size;
            box.offset = bodyRenderer.sprite.bounds.center;
        }
    }

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Update()
    {
        // Sürükleme sırasında parçayı mouse'u takip ettir
        if (isDragging)
        {
            HandleDragging();
        }

        // Seçili parçaya flip komutu (F tuşu)
        if (isSelected && Input.GetKeyDown(KeyCode.F))
        {
            FlipFace();
        }

        // Escape ile bırak (seçimi iptal et)
        if (isSelected && Input.GetKeyDown(KeyCode.Escape))
        {
            Deselect();
        }
    }

    // ── Mouse Olayları ────────────────────────────────────────

    private void OnMouseDown()
    {
        // Sol tık ile parçayı tıklayınca
        StartDragging();
    }

    private void OnMouseUp()
    {
        // Mouse bırakınca
        if (isDragging)
            StopDragging();
    }

    // ── Sürükleme Mantığı ─────────────────────────────────────

    private void StartDragging()
    {
        isDragging = true;
        wasEverDragged = true;
        isSelected = true;
        AudioManager.Instance.PlayOneShotSFX("LegoClick");
        // Eğer parça daha önce grid'e yerleşmişse grid'den kaldır
        if (isPlaced)
        {
            LegoGrid.Instance.UnregisterPiece(this);
            isPlaced = false;
        }

        // Mouse ile parça arasındaki offset'i hesapla
        // (parçanın tam pivot noktasına değil, tıklanan noktaya göre)
        Vector3 mouseWorld = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorld;

        // Drag öncesi sorting order'ları yedekle (drop iptal olursa geri yüklenecek)
        originalBodyOrder = bodyRenderer.sortingOrder;
        if (faceRenderer != null)
            originalFaceOrder = faceRenderer.sortingOrder;

        // Sürükleme sırasında en üstte görünsün
        bodyRenderer.sortingOrder = 999;
        if (faceRenderer != null)
            faceRenderer.sortingOrder = 1000;

        // PlacementGuide'ı aktifleştir
        if (placementGuide != null)
            placementGuide.Show(data);
    }

    private void HandleDragging()
    {
        //Debug.Log($"[GRID] instance: {LegoGrid.Instance.GetInstanceID()} | pos: {LegoGrid.Instance.transform.position} | cellX:{LegoGrid.Instance.cellSizeX} cellY:{LegoGrid.Instance.cellSizeY}");
        // Parçayı mouse pozisyonuna taşı (offset ile)
        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector3 dragLift = new Vector3(0, dragLiftAmount, 0); // Y'de yukarı kaydır
        transform.position = mouseWorld + dragOffset + dragLift;

        // Placement guide'ı güncelle: en yakın snap noktasını bul
        if (placementGuide != null)
        {
            // Mouse'un gerçek hedef pozisyonu (lift olmadan)
            Vector3 target = mouseWorld + dragOffset;

            var (snapPos, cell, isValid, isStacking) = LegoGrid.Instance.GetSmartSnapPoint(
                target, data, this);
            //Debug.Log($"[DRAG]  pos={transform.position} | snapPos={snapPos} | cell={cell} | valid={isValid}");
            // Parça yerleşince snapPos + spriteOffset'e gidiyor → ghost da öyle göstersin
            placementGuide.UpdateGuide(snapPos + (Vector3)EffectiveSpriteOffset, isValid, isRotated, isStacking);
        }
    }

    private void StopDragging()
    {
        //Debug.Log($"[GRID] instance: {LegoGrid.Instance.GetInstanceID()} | pos: {LegoGrid.Instance.transform.position} | cellX:{LegoGrid.Instance.cellSizeX} cellY:{LegoGrid.Instance.cellSizeY}");
        isDragging = false;
        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector3 target = mouseWorld + dragOffset;

        // En yakın snap noktasına yerleştir
        var (snapPos, cell, isValid, isStacking) = LegoGrid.Instance.GetSmartSnapPoint(
            target, data, this);
        //Debug.Log($"[STOP] çağıran: {gameObject.name} | placed parça sayısı kontrol...");
        // var grid = LegoGrid.Instance;
        // var field = typeof(LegoGrid).GetField("occupiedCells", 
        //     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // var dict = field.GetValue(grid) as System.Collections.IDictionary;
        //Debug.Log($"[STOP] grid'de dolu hücre sayısı: {dict.Count}");
        //Debug.Log($"[STOP]  pos={transform.position} | snapPos={snapPos} | cell={cell} | valid={isValid}");


        if (isValid)
        {
            // Snap pozisyonuna yerleş
            // spriteOffset: sprite'ın pivot'u sol-alt değilse düzelt
            transform.position = snapPos + (Vector3)EffectiveSpriteOffset;

            // Grid'e kaydet
            LegoGrid.Instance.RegisterPiece(this, cell.x, cell.y, cell.z);
            isPlaced = true;

            // Sorting order'ı güncelle (isometrik derinlik)
            UpdateSortingOrder(cell.x, cell.y, cell.z);
        }
        else
        {
            // Geçersiz pozisyon — scatter "evine" geri dön
            // Parça SİLİNMEZ, sahnede kalır, sadece eski yerine ışınlanır
            LegoBuilder.Instance.ReturnToScatter(this);
        }
        AudioManager.Instance.PlayOneShotSFX("LegoClick");

        // PlacementGuide'ı gizle
        if (placementGuide != null)
            placementGuide.Hide();
    }

    // ── Flip Mantığı ─────────────────────────────────────────

    /// <summary>
    /// Yüzü yatay çevirir.
    /// Sadece faceRenderer'ın flipX özelliği değişir — yeni asset gerekmez.
    /// Gövde (body) olduğu gibi kalır, çünkü isometrik gövde simetrik.
    /// </summary>
    public void FlipFace()
    {
        isFacingRight = !isFacingRight;
        isRotated = !isRotated;  // ← bu satır eklendi

        if (faceRenderer != null) 
            faceRenderer.flipX = !isFacingRight;
    }

    // ── Seçim Yönetimi ────────────────────────────────────────

    public void Select()
    {
        isSelected = true;
        // Seçili parçayı vurgula (hafif renk değişimi)
        bodyRenderer.color = new Color(
            data.pieceColor.r * 1.2f,
            data.pieceColor.g * 1.2f,
            data.pieceColor.b * 1.2f,
            1f
        );
    }

    public void Deselect()
    {
        isSelected = false;
        bodyRenderer.color = data.pieceColor;
    }

    // ── Yardımcı Fonksiyonlar ─────────────────────────────────

    /// <summary>
    /// Mouse'un dünya koordinatını döner (kamera Z=-10 varsayılır)
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mouseScreen);
    }

    /// <summary>
    /// İsometrik sorting order hesabı.
    /// col + row büyük olan daha "arkada" → daha küçük order
    /// layer büyük olan daha "önde" → daha büyük order
    /// </summary>
    private void UpdateSortingOrder(int col, int row, int layer)
    {
        // Basit formül: önce layer'ı öne al, sonra col+row ile ince ayar
        int order = layer * 1000 - (col + row) * 10;
        bodyRenderer.sortingOrder = order;
        if (faceRenderer != null)
            faceRenderer.sortingOrder = order + 1; // Yüz her zaman gövdenin önünde
    }

    /// <summary>
    /// Sorting order'ı drag öncesi haline döndürür.
    /// Scatter'a iade edildiğinde çağrılır.
    /// </summary>
    public void ResetSortingOrder()
    {
        bodyRenderer.sortingOrder = originalBodyOrder;
        if (faceRenderer != null)
            faceRenderer.sortingOrder = originalFaceOrder;
    }

    // ── Public Getter'lar ─────────────────────────────────────

    public bool IsPlaced => isPlaced;
    public bool IsFacingRight => isFacingRight;
    public SpriteRenderer BodyRenderer => bodyRenderer;
}

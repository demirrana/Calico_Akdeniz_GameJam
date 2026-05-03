using UnityEngine;

// ============================================================
// PlacementGuide.cs
// Parça sürüklenirken nereye oturacağını gösteren kılavuz.
// 
// Nasıl çalışır:
//   - Sürüklenen parçanın şeffaf bir kopyasını gösterir
//   - Geçerli pozisyon → yeşil tint
//   - Geçersiz pozisyon (dolu/sınır dışı) → kırmızı tint
//   - Sadece parça sürüklenirken aktif olur
// ============================================================

public class PlacementGuide : MonoBehaviour
{
    // ── Inspector Ayarları ────────────────────────────────────

    [Header("Renk Ayarları")]
    [Tooltip("Yerleştirme geçerliyken ghost rengi")]
    public Color validColor = new Color(0f, 1f, 0.3f, 0.4f);   // Yeşil şeffaf

    [Tooltip("Yerleştirme geçersizken ghost rengi")]
    public Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.4f); // Kırmızı şeffaf

    [Header("Guide Sprite Renderer")]
    // Bu, Guide objesinin kendi SpriteRenderer'ı
    // Hiyerarşi: PlacementGuide > GhostSprite(SpriteRenderer)
    [Tooltip("Kılavuz ghost sprite renderer'ı (child objede olmalı)")]
    public SpriteRenderer ghostRenderer;

    // ── Durum ─────────────────────────────────────────────────
    private bool isActive = false;

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Awake()
    {
        // Başlangıçta gizli
        if (ghostRenderer != null)
            ghostRenderer.enabled = false;

        // Ghost renderer belirtilmemişse child'da ara
        if (ghostRenderer == null)
            ghostRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // ── Public API ────────────────────────────────────────────

    /// <summary>
    /// Sürükleme başladığında çağrılır.
    /// Sürüklenen parçanın sprite'ını ghost olarak ayarlar.
    /// </summary>
    public void Show(LegoPieceData data)
    {
        if (ghostRenderer == null) return;

        ghostRenderer.sprite = data.pieceSprite;
        ghostRenderer.enabled = true;
        isActive = true;
    }

    /// <summary>
    /// Her frame sürükleme sırasında çağrılır.
    /// Snap pozisyonunu ve rengi günceller.
    /// </summary>
    public void UpdateGuide(Vector3 snapPos, bool isValid, bool isRotated = false)
    {
        if (!isActive || ghostRenderer == null) return;

        // Ghost sprite'ı snap noktasına taşı
        transform.position = snapPos;

        // Geçerlilik durumuna göre rengi değiştir
        ghostRenderer.color = isValid ? validColor : invalidColor;

         // Parça döndürülmüşse ghost da yatay flip
        ghostRenderer.flipX = isRotated;
    }

    /// <summary>
    /// Sürükleme bitince gizle
    /// </summary>
    public void Hide()
    {
        if (ghostRenderer != null)
            ghostRenderer.enabled = false;

        isActive = false;
    }
}

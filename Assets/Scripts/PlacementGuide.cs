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
    public Color validColor = new Color(0f, 1f, 0.3f, 0.4f);     // yeşil = zemin
    public Color stackingColor = new Color(1f, 0.9f, 0f, 0.4f);  // sarı = üste koyma
    public Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.4f); // kırmızı = geçersiz

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
        AudioManager.Instance.PlayOneShotSFX("LegosSound");
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
    public void UpdateGuide(Vector3 snapPos, bool isValid, bool isRotated = false, bool isStacking = false)
    {
        if (!isActive || ghostRenderer == null) return;

        // Ghost sprite'ı snap noktasına taşı
        transform.position = snapPos;

        // Geçerlilik durumuna göre rengi değiştir
        if (!isValid)
            ghostRenderer.color = invalidColor;
        else if (isStacking)
            ghostRenderer.color = stackingColor;
        else
            ghostRenderer.color = validColor;

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

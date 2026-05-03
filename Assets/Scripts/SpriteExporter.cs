using UnityEngine;
using System.IO;
using System.Collections.Generic;

// ============================================================
// SpriteExporter.cs
// Sahnedeki tüm lego parçalarını tek bir Sprite/PNG olarak export eder.
//
// Nasıl çalışır:
//   1. Geçici bir RenderTexture oluşturur
//   2. Sadece lego parçalarını render eden ayrı bir kamera kullanır
//   3. RenderTexture'ı Texture2D'ye kopyalar
//   4. PNG olarak diske yazar (veya oyun içi Sprite olarak döner)
//
// Kurulum:
//   - Sahnede ayrı bir "ExportCamera" objesi oluştur
//   - Bu kamerayı exportCamera alanına ata
//   - Kameranın CullingMask'ini sadece "LegoPiece" layer'ına ayarla
// ============================================================

public class SpriteExporter : MonoBehaviour
{
    [Header("Export Kamerası")]
    [Tooltip("Sadece lego parçalarını gören ayrı kamera")]
    public Camera exportCamera;

    [Header("Çıktı Ayarları")]
    [Tooltip("Export edilen texture'ın genişliği (piksel)")]
    public int textureWidth = 512;

    [Tooltip("Export edilen texture'ın yüksekliği (piksel)")]
    public int textureHeight = 512;

    [Tooltip("PNG dosyasının kaydedileceği dizin (Application.dataPath göreceli)")]
    public string outputPath = "ExportedSprites";

    [Tooltip("Dosya adı (uzantısız)")]
    public string fileName = "LegoModel";

    [Header("Arka Plan")]
    [Tooltip("Export arka planı şeffaf mı olsun? (Alpha = 0)")]
    public bool transparentBackground = true;

    // ── Export Fonksiyonu ─────────────────────────────────────

    /// <summary>
    /// Verilen parça listesini render eder ve PNG olarak kaydeder.
    /// Aynı zamanda Unity Sprite olarak döner (UI'da kullanmak için).
    /// </summary>
    public Sprite Export(List<LegoPiece> pieces)
    {
        if (exportCamera == null)
        {
            Debug.LogError("SpriteExporter: exportCamera atanmamış!");
            return null;
        }

        // ── 1. Kamerayı tüm parçaları kapsayacak şekilde konumlandır ──
        FrameAllPieces(pieces);

        // ── 2. RenderTexture oluştur ──────────────────────────────────
        RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        renderTexture.antiAliasing = 4; // MSAA x4 ile temiz kenarlar

        // ── 3. Kamerayı RenderTexture'a yönlendir ─────────────────────
        exportCamera.targetTexture = renderTexture;

        // Arka plan ayarı
        if (transparentBackground)
        {
            exportCamera.clearFlags = CameraClearFlags.SolidColor;
            exportCamera.backgroundColor = new Color(0, 0, 0, 0); // Şeffaf siyah
        }

        // Render et
        exportCamera.Render();

        // ── 4. RenderTexture → Texture2D ─────────────────────────────
        RenderTexture.active = renderTexture;

        Texture2D exportTexture = new Texture2D(textureWidth, textureHeight,
            TextureFormat.RGBA32, false);

        // GPU'dan CPU'ya piksel kopyala
        exportTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        exportTexture.Apply();

        // ── 5. Temizlik ───────────────────────────────────────────────
        RenderTexture.active = null;
        exportCamera.targetTexture = null;
        Destroy(renderTexture);

        // ── 6. PNG olarak kaydet ──────────────────────────────────────
        string savedPath = SaveToPNG(exportTexture);
        Debug.Log($"SpriteExporter: Model kaydedildi → {savedPath}");

        // ── 7. Sprite olarak döndür (oyun içi kullanım için) ──────────
        Sprite exportedSprite = Texture2DToSprite(exportTexture);

        return exportedSprite;
    }

    // ── Yardımcı: Kamerayı tüm parçalara odakla ──────────────

    private void FrameAllPieces(List<LegoPiece> pieces)
    {
        if (pieces == null || pieces.Count == 0) return;

        // Tüm parçaların bounding box'ını hesapla
        Bounds totalBounds = new Bounds(pieces[0].transform.position, Vector3.zero);
        foreach (var piece in pieces)
        {
            // Her parçanın sprite sınırlarını ekle
            if (piece.BodyRenderer != null && piece.BodyRenderer.sprite != null)
            {
                // Sprite'ın dünya uzayındaki bounds'u
                Bounds spriteBounds = piece.BodyRenderer.bounds;
                totalBounds.Encapsulate(spriteBounds);
            }
        }

        // Kamerayı bu bounds'un merkezine taşı
        Vector3 camPos = totalBounds.center;
        camPos.z = exportCamera.transform.position.z; // Z sabit kalsın
        exportCamera.transform.position = camPos;

        // Orthographic size'ı ayarla (biraz padding ekle)
        float padding = 1.2f; // %20 boşluk
        float sizeByHeight = totalBounds.size.y * 0.5f * padding;
        float sizeByWidth = totalBounds.size.x * 0.5f * padding
                            * ((float)textureHeight / textureWidth); // Aspect ratio düzelt

        exportCamera.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }

    // ── Yardımcı: PNG Kaydet ──────────────────────────────────

    private string SaveToPNG(Texture2D texture)
    {
        // Çıktı dizinini oluştur
        string fullDir = Path.Combine(Application.dataPath, outputPath);
        if (!Directory.Exists(fullDir))
            Directory.CreateDirectory(fullDir);

        // Timestamp ile benzersiz dosya adı
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fullPath = Path.Combine(fullDir, $"{fileName}_{timestamp}.png");

        // PNG baytlarını yaz
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);

#if UNITY_EDITOR
        // Editor'da Asset Database'i yenile
        UnityEditor.AssetDatabase.Refresh();
#endif

        return fullPath;
    }

    // ── Yardımcı: Texture2D → Sprite ─────────────────────────

    private Sprite Texture2DToSprite(Texture2D texture)
    {
        // Tüm texture'ı tek sprite olarak sar
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), // Pivot: merkez
            100f                      // Pixels per unit
        );
    }
}

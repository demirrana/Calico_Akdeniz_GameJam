using UnityEngine;

// ============================================================
// LegoPieceData.cs
// ScriptableObject: Her lego parça tipinin verilerini tutar.
// Unity'de sağ tık > Create > LegoBuilder > LegoPieceData ile
// yeni parça tipi oluşturabilirsin.
// ============================================================

[CreateAssetMenu(menuName = "LegoBuilder/LegoPieceData", fileName = "New LegoPieceData")]
public class LegoPieceData : ScriptableObject
{
    [Header("Parça Kimliği")]
    public string pieceName;         // Örn: "2x2 Küp", "2x4 Dikdörtgen"

    [Header("Grid Boyutları")]
    // Parçanın kaç grid hücresi kapladığı (stud sayısı)
    public int gridWidth;            // X ekseninde kaç stud (2x2 için 2, 2x4 için 4 gibi)
    public int gridHeight;           // Z ekseninde kaç stud (isometrik "derinlik")
    // NOT: Tüm parçalar 1 katman yüksekliğinde (Y ekseninde)

    [Header("Sprite'lar")]
    public Sprite pieceSprite;       // Ana parça sprite'ı (isometrik görünüm)
    public Sprite faceSpriteRight;   // Yüzün sağa baktığı hali
    // Sol bakış için aynı sprite'ı flipX ile kullanacağız (ayrı asset gerekmez)

    [Header("Pivot / Offset Ayarları")]
    // Sprite'ın sol alt köşesinin grid origin'e göre piksel offseti
    // İsometrik görünümde parçanın tam oturması için ince ayar
    public Vector2 spriteOffset;
    
    [Tooltip("Parça 90° döndürülmüş halinin ek offseti (rotated halde uygulanır)")]
    public Vector2 spriteOffsetRotated;

    [Header("Görsel")]
    public Color pieceColor = Color.white;   // Parçanın rengi (tint)
}

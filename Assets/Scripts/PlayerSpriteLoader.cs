using UnityEngine;

public class PlayerSpriteLoader : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // BuilderScene'de export edilen sprite'ı yükle
        if (SpriteExporter.LastExportedSprite != null)
        {
            sr.sprite = SpriteExporter.LastExportedSprite;
            Debug.Log("[PlayerSpriteLoader] Lego sprite yüklendi.");
        }
        else
        {
            Debug.LogWarning("[PlayerSpriteLoader] Export edilmiş sprite bulunamadı.");
        }
    }
}
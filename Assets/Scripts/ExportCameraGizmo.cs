using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ExportCameraGizmo : MonoBehaviour
{
    public Color frameColor = new Color(1f, 0.5f, 0f, 1f);

    private void OnDrawGizmos()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic) return;

        // SpriteExporter'dan output texture oranını al
        SpriteExporter exporter = GetComponent<SpriteExporter>();
        if (exporter == null) return;

        float texAspect = (float)exporter.textureWidth / exporter.textureHeight;

        // Kamera Y'sine sığacak şekilde dikdörtgen hesapla
        // (export render böyle çalışıyor: orthographic size = halfH sabit, halfW oran ile)
        float halfH = cam.orthographicSize;
        float halfW = halfH * texAspect;

        Vector3 c = transform.position;
        Vector3 tl = new Vector3(c.x - halfW, c.y + halfH, 0);
        Vector3 tr = new Vector3(c.x + halfW, c.y + halfH, 0);
        Vector3 bl = new Vector3(c.x - halfW, c.y - halfH, 0);
        Vector3 br = new Vector3(c.x + halfW, c.y - halfH, 0);

        Gizmos.color = frameColor;
        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }
}
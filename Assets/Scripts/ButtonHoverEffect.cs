using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ============================================================
// ButtonHoverEffect.cs
// Mouse butonun üstüne gelince:
//   - Hafif büyütür (scale)
//   - Parlatır (color tint daha açık)
//   - Çıkınca eski haline döner
// 
// Smooth lerp ile yumuşak geçiş
// ============================================================

[RequireComponent(typeof(Image))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Büyüme")]
    public float hoverScale = 1.1f;       // %10 büyür
    public float scaleSpeed = 10f;        // Lerp hızı

    [Header("Parlaklık")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1.2f, 1.2f, 1.2f, 1f); // hafif parlak

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Color targetColor;
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        targetColor = normalColor;
        img.color = normalColor;
    }

    private void Update()
    {
        // Smooth scale interpolation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

        // Smooth color interpolation
        img.color = Color.Lerp(img.color, targetColor, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        targetColor = normalColor;
    }
}
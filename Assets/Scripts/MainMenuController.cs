using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Sahne")]
    public string builderSceneName = "LegoBuildScene";

    [Header("Buton Animasyonu")]
    [Tooltip("Butonların animatör'ü — Play tuşuna basınca trigger gönderilir")]
    public Animator buttonsAnimator;
    [Tooltip("Animator'a gönderilecek trigger ismi")]
    public string buttonsSlideTrigger = "SlideDown";
    [Tooltip("Buton animasyonu kaç saniye sürüyor (Animator'daki clip uzunluğu)")]
    public float buttonsAnimationDuration = 1f;

    [Header("Background Netleşme")]
    [Tooltip("Blur material — Soft Mix değişecek")]
    public Material blurMaterial;
    public float softMixStart = 0.7f;
    public float softMixEnd = 0f;
    [Tooltip("Netleşme süresi (saniye)")]
    public float blurFadeDuration = 1.5f;

    [Header("Geçiş")]
    [Tooltip("Blur netleştikten sonra çalacak ses (AudioManager üzerinden)")]
    public string transitionSFX1 = "Sound";
    public string transitionSFX2 = "Sound";
    [Tooltip("Sesin uzunluğu (saniye) — bittikten sonra sahne geçer")]
    public float transitionSFXDuration = 2f;

    private bool isPlaying = false;

    private void Start()
    {
        AudioManager.Instance.PlayMusic("MainMenu");
        // Material başlangıç değerini ayarla
        if (blurMaterial != null)
            blurMaterial.SetFloat("_MixAmount", softMixStart);
    }

    public void OnPlayClicked()
    {
        AudioManager.Instance.StopMusic();
        if (isPlaying) return;
        isPlaying = true;
        StartCoroutine(PlaySequence());
    }

    public void OnHowToPlayClicked()
    {
        Debug.Log("[MainMenu] Nasıl oynanır basıldı");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── Play Sırası ──────────────────────────────────────────
    private IEnumerator PlaySequence()
    {
        // 1. Buton animasyonunu tetikle
        if (buttonsAnimator != null)
            buttonsAnimator.SetTrigger(buttonsSlideTrigger);

        // 2. Buton animasyonunun bitmesini bekle
        yield return new WaitForSeconds(buttonsAnimationDuration);

        // 3. Blur'dan nete geçiş
        float elapsed = 0f;
        while (elapsed < blurFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / blurFadeDuration);
            // Ease-out
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (blurMaterial != null)
                blurMaterial.SetFloat("_MixAmount", Mathf.Lerp(softMixStart, softMixEnd, eased));

            yield return null;
        }
        // 4. Sesi çal
        if (!string.IsNullOrEmpty(transitionSFX1) && AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShotSFX(transitionSFX1);
        // Sesin bitmesini bekle
        yield return new WaitForSeconds(transitionSFXDuration);
        if (!string.IsNullOrEmpty(transitionSFX2) && AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShotSFX(transitionSFX2);
        yield return new WaitForSeconds(transitionSFXDuration);
        // Final değer sabitle
        if (blurMaterial != null)
            blurMaterial.SetFloat("_MixAmount", softMixEnd);

        SceneManager.LoadScene(builderSceneName);
    }

    // Editor'de material değerinin kalıcı olmaması için
    private void OnApplicationQuit()
    {
        if (blurMaterial != null)
            blurMaterial.SetFloat("_MixAmount", softMixStart);
    }
}
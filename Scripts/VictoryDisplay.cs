using UnityEngine;
using TMPro; // TextMeshPro kullanýyoruz
using System.Collections;

public class VictoryDisplay : MonoBehaviour
{
    [Header("Referanslar")]
    public TextMeshProUGUI victoryText; // VR World Space ise TextMeshPro kullanýn
    public CanvasGroup canvasGroup;     // Yazýnýn Alpha (þeffaflýk) kontrolü için

    [Header("Ayarlar")]
    public float fadeDuration = 1.0f;   // Giriþ/Çýkýþ hýzý
    public float displayDuration = 2.0f; // Ekranda kalma süresi

    private void Start()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0; // Baþlangýçta görünmez yap
    }

    public void ShowVictory(int strokeCount)
    {
        victoryText.text = strokeCount + " VURUÞTA ÝSABET ";
        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        // 1. Smooth Giriþ (Fade In)
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;

        // 2. Bekleme Süresi
        yield return new WaitForSeconds(displayDuration);

        // 3. Smooth Çýkýþ (Fade Out)
        elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}
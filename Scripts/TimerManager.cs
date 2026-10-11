using UnityEngine;
using TMPro;
using System.Collections;

public class TimerManager : MonoBehaviour
{
    public static TimerManager instance;

    [Header("Zaman Ayarlarý")]
    [Tooltip("Toplam geri sayým süresi")]
    public float countdown = 30f;
    [Tooltip("Geri sayým kaç saniye kala ekranda görünsün?")]
    public float showLastSeconds = 10f;

    [Header("Kalan süre")]
    public float countdownTimer; // Kalan süreyi buradan görebilirsin

    [Header("UI Referanslarý")]
    public TextMeshProUGUI timerText;
    public CanvasGroup timerCanvasGroup;

    private bool isTimerRunning = false;
    private bool isTimeOver = false;

    void Awake()
    {
        instance = this;
        if (timerCanvasGroup != null) timerCanvasGroup.alpha = 0;
    }

    public void StartTimer()
    {
        countdownTimer = countdown; // Süreyi baþtan baþlat
        isTimerRunning = true;
        isTimeOver = false;
        if (timerCanvasGroup != null) timerCanvasGroup.alpha = 0;
        if (timerText != null) timerText.text = "";
    }

    void Update()
    {
        if (!isTimerRunning || isTimeOver) return;

        countdownTimer -= Time.deltaTime;

        // Sadece son belirlenen saniyelerde UI'yý göster
        if (countdownTimer <= showLastSeconds)
        {
            if (timerCanvasGroup != null && timerCanvasGroup.alpha < 1)
                timerCanvasGroup.alpha = 1;

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(Mathf.Max(0, countdownTimer)).ToString();
        }

        // Süre dolduðunda
        if (countdownTimer <= 0)
        {
            StartCoroutine(TimeOverSequence());
        }
    }

    public bool IsTimeOver() => isTimeOver;

    IEnumerator TimeOverSequence()
    {
        isTimeOver = true;
        isTimerRunning = false;
        countdownTimer = 0;

        if (timerText != null)
        {
            timerText.text = "SÜRE BÝTTÝ!";
            yield return StartCoroutine(FadeCanvas(timerCanvasGroup, timerCanvasGroup.alpha, 1f, 0.3f));
        }

        yield return new WaitForSeconds(2.0f);
        yield return StartCoroutine(FadeCanvas(timerCanvasGroup, 1, 0, 0.5f));

        LevelEndManager lem = FindFirstObjectByType<LevelEndManager>();
        if (lem != null) lem.ForceNextLevel();
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null) yield break;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}
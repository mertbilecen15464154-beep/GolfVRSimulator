using UnityEngine;
using System.Collections;
using TMPro;

public class LevelStartManager : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform playerRig;
    public Transform golfBall;
    public GameObject golfClub;
    public GameObject golfClubHead;

    [Header("UI & Ses")]
    public TextMeshProUGUI readyText;
    public TextMeshProUGUI startText;
    public AudioSource audioSource;
    public AudioClip readySound;
    public AudioClip startSound;

    [Header("Ayarlar")]
    public float transitionDuration = 3.0f;
    public float ballDistance = 1.0f;
    public float fadeDuration = 0.5f;

    private CanvasGroup readyCG;
    private CanvasGroup startCG;
    private LevelManager lvManager;

    void Start()
    {
        lvManager = FindFirstObjectByType<LevelManager>();

        if (readyText != null)
        {
            readyCG = readyText.GetComponentInParent<CanvasGroup>();
            if (readyCG != null) readyCG.alpha = 0;
        }
        if (startText != null)
        {
            startCG = startText.GetComponentInParent<CanvasGroup>();
            if (startCG != null) startCG.alpha = 0;
        }

        RestartIntro();
    }

    public void RestartIntro()
    {
        StopAllCoroutines();
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // 1. Sopalarý baþlangýçta kapat
        if (golfClub != null) golfClub.SetActive(false);
        if (golfClubHead != null) golfClubHead.SetActive(false);

        if (lvManager == null) lvManager = FindFirstObjectByType<LevelManager>();

        // LevelManager'ýn harita ve deliði atamasýný bekle
        while (lvManager == null || lvManager.holeTransform == null)
        {
            yield return null;
        }

        // Topun hýzýný sýfýrla
        Rigidbody rb = golfBall.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 2. KAMERA DELÝÐE ODAKLANIR
        Transform targetHole = lvManager.holeTransform;
        Vector3 holeViewPos = targetHole.position + (Vector3.up * 1.5f) - (targetHole.forward * 2.5f);
        playerRig.SetPositionAndRotation(holeViewPos, Quaternion.LookRotation((targetHole.position - holeViewPos).normalized));

        // "HAZIR" Yazýsý
        if (audioSource != null && readySound != null) audioSource.PlayOneShot(readySound);
        yield return StartCoroutine(FadeCanvas(readyCG, 0, 1, fadeDuration));
        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(FadeCanvas(readyCG, 1, 0, fadeDuration));

        // 3. TOPA YUMUÞAK GEÇÝÞ
        playerRig.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
        Vector3 directionFromBallToHole = (targetHole.position - golfBall.position).normalized;
        Vector3 targetPos = golfBall.position - (directionFromBallToHole * ballDistance);
        targetPos.y = golfBall.position.y;
        Quaternion targetRot = Quaternion.Euler(0, Quaternion.LookRotation(directionFromBallToHole).eulerAngles.y, 0);

        float elapsed = 0;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / transitionDuration);
            playerRig.SetPositionAndRotation(Vector3.Lerp(startPos, targetPos, t), Quaternion.Slerp(startRot, targetRot, t));
            yield return null;
        }

        // 4. BAÞLA VE AKTÝVASYON
        if (audioSource != null && startSound != null) audioSource.PlayOneShot(startSound);

        if (ScreenFader.instance != null)
            StartCoroutine(ScreenFader.instance.FadeToClear());

        // Sopalarý ve vuruþ sistemini aç
        if (golfClub != null) golfClub.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (golfClubHead != null) golfClubHead.SetActive(true);
        rb.WakeUp();

        // --- KRÝTÝK: BOTLARI BURADA TETÝKLÝYORUZ ---
        GolfBot[] allBots = FindObjectsByType<GolfBot>(FindObjectsSortMode.None);
        foreach (GolfBot bot in allBots)
        {
            bot.ActivateBot();
        }

        // "BAÞLA" Yazýsý
        yield return StartCoroutine(FadeCanvas(startCG, 0, 1, fadeDuration));
        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(FadeCanvas(startCG, 1, 0, fadeDuration));

        // Teleport scriptini aktif et
        TeleportToBall tpScript = FindFirstObjectByType<TeleportToBall>();
        if (tpScript != null) tpScript.enabled = true;
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
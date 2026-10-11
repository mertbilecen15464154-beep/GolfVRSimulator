using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class LevelEndManager : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject golfClub;
    public Transform playerRig;
    public Transform golfBall;
    public VictoryDisplay victoryDisplay;

    [Header("Kamera Ayarlarý")]
    public float heightAboveHole = 2f;
    public float distanceBehindHole = 2.0f;
    public float transitionDuration = 2.0f;
    public InputActionProperty thumbstickAction;
    public float rotationSpeed = 100f;

    [Header("Bot Takip Ayarlarý")]
    public int totalPlayers = 6; // Siz + 5 Bot
    private int finishedPlayerCount = 0;

    private bool levelFinished = false;
    private bool canControlCamera = false;
    private LevelManager lvManager;

    void Start()
    {
        // FindObjectOfType yerine yeni ve performanslý yöntem
        lvManager = Object.FindFirstObjectByType<LevelManager>();
    }

    public void EnableCameraControl()
    {
        canControlCamera = true;
        levelFinished = false;
    }

    // --- YENÝ EKLENEN FONKSÝYON: Botlar veya Siz her girdiðinde çaðrýlýr ---
    public void PlayerFinished()
    {
        finishedPlayerCount++;
        Debug.Log("Bir oyuncu bitirdi. Toplam: " + finishedPlayerCount);

        // Herkes bitirdiðinde geçiþ sürecini baþlat
        if (finishedPlayerCount >= totalPlayers)
        {
            StartCoroutine(FinalTransitionSequence());
        }
    }

    // --- YENÝ EKLENEN FONKSÝYON: LevelManager tarafýndan her harita baþýnda çaðrýlýr ---
    public void ResetFinishedCount()
    {
        finishedPlayerCount = 0;
        levelFinished = false;
    }

    public void FinishLevel()
    {
        if (levelFinished || lvManager == null) return;
        levelFinished = true; // Sizin bitiþ bayraðýnýz

        if (golfClub != null) golfClub.SetActive(false);

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.MapCompleted();
            if (victoryDisplay != null)
                victoryDisplay.ShowVictory(ScoreManager.instance.GetCurrentStrokes());
        }

        StartCoroutine(SmoothCameraTransition());
    }

    IEnumerator SmoothCameraTransition()
    {
        playerRig.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);
        Transform targetHole = lvManager.holeTransform;

        Vector3 targetPos = targetHole.position + (Vector3.up * heightAboveHole) - (targetHole.forward * distanceBehindHole);
        Quaternion targetRot = Quaternion.LookRotation(targetHole.forward);

        float elapsed = 0;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / transitionDuration);
            playerRig.SetPositionAndRotation(Vector3.Lerp(startPos, targetPos, t), Quaternion.Slerp(startRot, targetRot, t));
            yield return null;
        }

        canControlCamera = true;
    }

    IEnumerator FinalTransitionSequence()
    {
        // Son bot girdikten sonra 1.5 saniye bekle ki kaos hemen bitmesin
        yield return new WaitForSeconds(1.5f);

        if (ScoreManager.instance.IsGameFinished())
        {
            if (ScreenFader.instance != null)
                yield return StartCoroutine(ScreenFader.instance.FadeToBlack());

            UnityEngine.SceneManagement.SceneManager.LoadScene("FinalScene");
        }
        else
        {
            if (ScreenFader.instance != null)
                yield return StartCoroutine(ScreenFader.instance.FadeToBlack());

            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    public void ForceNextLevel()
    {
        // Skor manager'da haritayý tamamlanmýþ say ve yeni haritaya geç
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.MapCompleted();
        }
        StartCoroutine(FinalTransitionSequence());
    }
    void Update()
    {
        if (!canControlCamera || lvManager == null) return;

        Vector2 stickValue = thumbstickAction.action.ReadValue<Vector2>();
        if (Mathf.Abs(stickValue.x) > 0.1f)
        {
            Transform targetTransform = levelFinished ? lvManager.holeTransform : golfBall;
            if (targetTransform != null)
            {
                playerRig.RotateAround(targetTransform.position, Vector3.up, stickValue.x * rotationSpeed * Time.deltaTime);
            }
        }
    }
}
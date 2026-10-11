using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI Ayarlarý")]
    public TextMeshProUGUI currentMapScoreText; // Sizin vuruþ sayýnýzýn yazdýðý Text

    // Statik veriler sahne deðiþse de bellekte kalýr
    public static int totalStrokes = 0;
    public static int completedMaps = 0;

    private int currentStrokes = 0; // Sizin o haritadaki vuruþunuz
    private readonly int maxMapCount = 3;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler arasý objeyi korur
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    // Sizin vuruþunuzu artýrýr
    public void AddStroke()
    {
        currentStrokes++;
        UpdateUI();
    }

    public int GetCurrentStrokes() => currentStrokes;
    public int GetTotalStrokes() => totalStrokes; // Final sahnesi için gerekli

    // LevelManager harita bitince bunu çaðýrýr
    public void MapCompleted()
    {
        totalStrokes += currentStrokes;
        completedMaps++;
    }

    // --- KRÝTÝK FONKSÝYON: LevelManager'daki hatayý çözer ---
    public void ResetScoreForNextMap()
    {
        currentStrokes = 0;
        UpdateUI();
    }

    public int GetFinalTotalScore()
    {
        return totalStrokes;
    }

    public bool IsGameFinished() => completedMaps >= maxMapCount;

    // Oyunu tamamen resetlemek için (Final sahnesinden baþa dönerken)
    public static void ResetGameStatic()
    {
        totalStrokes = 0;
        completedMaps = 0;
    }

    private void UpdateUI()
    {
        if (currentMapScoreText != null)
            currentMapScoreText.text = currentStrokes.ToString();
    }
}
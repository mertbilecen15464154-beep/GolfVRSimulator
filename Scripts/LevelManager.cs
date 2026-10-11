using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public List<GameObject> tumBolumler;
    public Transform holeTransform;
    public Rigidbody ballRb;

    [Header("Bot Ayarlarý")]
    public GameObject botPrefab;
    private readonly List<GolfBot> activeBots = new();

    // --- KRÝTÝK DEÐÝÞÝKLÝK: STATÝK HAVUZ ---
    // Statik olduðu için sahne deðiþse de hangi haritalarýn kaldýðýný hatýrlar.
    private static List<int> oynanacakBolumIndexleri = new List<int>();
    private GameObject mevcutBolum;

    [Header("VR Ayarlarý")]
    public Transform xrOrigin;

    void Start()
    {        
        // Eðer havuz boþsa (oyun ilk kez açýldýysa veya tüm haritalar bittiyse) doldur
        if (oynanacakBolumIndexleri == null || oynanacakBolumIndexleri.Count == 0)
        {
            HavuzuYenile();
        }
        RastgeleBolumGetir();
    }

    public void HavuzuYenile()
    {
        oynanacakBolumIndexleri = new List<int>();
        for (int i = 0; i < tumBolumler.Count; i++)
        {
            oynanacakBolumIndexleri.Add(i);
        }
        Debug.Log("Bölüm havuzu yenilendi. Toplam harita: " + oynanacakBolumIndexleri.Count);
    }

    public void RastgeleBolumGetir()
    {
        // 1. ADIM: Bölüm Seçimi
        if (oynanacakBolumIndexleri.Count == 0) HavuzuYenile();

        // Önce sahnede açýk kalmýþ olabilecek tüm bölümleri kapat
        foreach (var bolum in tumBolumler) bolum.SetActive(false);

        // Havuzdan rastgele bir index seç
        int listeSirasi = Random.Range(0, oynanacakBolumIndexleri.Count);
        int secilenBolumIndex = oynanacakBolumIndexleri[listeSirasi];

        mevcutBolum = tumBolumler[secilenBolumIndex];
        mevcutBolum.SetActive(true);

        // SEÇÝLEN HARÝTAYI LÝSTEDEN SÝL (Bir daha gelmemesi için)
        oynanacakBolumIndexleri.RemoveAt(listeSirasi);
        Debug.Log("Harita Seçildi: " + mevcutBolum.name + " | Kalan Harita Sayýsý: " + oynanacakBolumIndexleri.Count);

        // 2. ADIM: Delik ve Baþlangýç Noktalarýný Bul
        holeTransform = mevcutBolum.transform.Find("Hole");
        Transform startPointsContainer = mevcutBolum.transform.Find("BallStartPositions");
        List<Transform> startPoints = new();

        foreach (Transform t in startPointsContainer) startPoints.Add(t);

        // 3. ADIM: Noktalarý Karýþtýr
        for (int i = 0; i < startPoints.Count; i++)
        {
            Transform temp = startPoints[i];
            int randomIndex = Random.Range(i, startPoints.Count);
            startPoints[i] = startPoints[randomIndex];
            startPoints[randomIndex] = temp;
        }

        // 4. ADIM: Oyuncuyu ve XR Origin'i Yerleþtir
        if (startPoints.Count > 0 && ballRb != null)
        {
            ballRb.gameObject.SetActive(true);
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;

            ballRb.position = startPoints[0].position;
            ballRb.rotation = startPoints[0].rotation;

            if (xrOrigin == null)
            {
                GameObject foundObj = GameObject.Find("XR Origin (XR Rig)");
                if (foundObj != null) xrOrigin = foundObj.transform;
            }

            if (xrOrigin != null)
            {
                xrOrigin.position = startPoints[0].position;
            }
        }

        // 5. ADIM: Botlarý Oluþtur
        foreach (var b in activeBots) if (b != null) Destroy(b.gameObject);
        activeBots.Clear();

        for (int i = 1; i < startPoints.Count && i <= 5; i++)
        {
            GameObject botObj = Instantiate(botPrefab, startPoints[i].position, Quaternion.identity);
            botObj.name = "GolfBall_Bot (" + i + ")";
            if (botObj.TryGetComponent<GolfBot>(out var botScript)) activeBots.Add(botScript);
        }

        // 6. ADIM: Sistemleri Sýfýrla
        LevelEndManager lem = FindFirstObjectByType<LevelEndManager>();
        if (lem != null) lem.ResetFinishedCount();

        // 7. ADIM: Zamanlayýcýyý Baþlat
        if (TimerManager.instance != null) TimerManager.instance.StartTimer();
    }
}
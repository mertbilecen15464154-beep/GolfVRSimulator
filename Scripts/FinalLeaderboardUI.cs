using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Sýralama yapmak için gerekli

public class FinalLeaderboardUI : MonoBehaviour
{
    [Header("Görsel Elemanlar")]
    public TextMeshProUGUI resultMessageText;
    public GameObject playerRowPrefab;
    public Transform listContainer;

    [Header("Oyuncu Bilgileri")]
    public Sprite defaultAvatar;

    // Oyuncu verilerini tutmak için basit bir sýnýf
    private class PlayerData
    {
        public string name;
        public int score;
        public bool isLocal;
    }

    void Start()
    {
        listContainer.gameObject.SetActive(false);
        if (resultMessageText != null) resultMessageText.gameObject.SetActive(true);

        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        // 1. VERÝ HAZIRLAMA: Oyuncularý ve Botlarý oluþtur
        List<PlayerData> allPlayers = new List<PlayerData>();

        // Kendi verin
        allPlayers.Add(new PlayerData
        {
            name = "Siz",
            score = ScoreManager.instance.GetFinalTotalScore(),
            isLocal = true
        });

        // 5 Tane Bot ekle (7 ile 15 vuruþ arasý rastgele)
        string[] botNames = { "GolfMaster", "LuckyShot", "EagleEye", "BogeyMan", "ProPlayer" };
        for (int i = 0; i < 5; i++)
        {
            allPlayers.Add(new PlayerData
            {
                name = botNames[i],
                score = Random.Range(7, 16), // 7 dahil, 16 hariç (yani 15 max)
                isLocal = false
            });
        }

        // 2. SIRALAMA: Skorlara göre küçükten büyüye diz (En az vuruþ yapan 1. olur)
        allPlayers = allPlayers.OrderBy(p => p.score).ToList();

        // 3. MESAJ BELÝRLEME: Senin kaçýncý olduðunu bul
        int myRank = allPlayers.FindIndex(p => p.isLocal) + 1;
        SetResultMessage(myRank);

        yield return new WaitForSeconds(1.0f);
        if (resultMessageText != null) resultMessageText.gameObject.SetActive(false);

        // 4. LÝSTEYÝ GÖSTERME: Sýrasýyla ekrana bas
        listContainer.gameObject.SetActive(true);

        float[] delays = { 0f, 0.3f, 0.3f, 0.3f, 0.3f, 0.2f }; // Ýstediðin gecikme süreleri

        for (int i = 0; i < allPlayers.Count; i++)
        {
            yield return new WaitForSeconds(delays[i]);
            CreateRow(i + 1, allPlayers[i].name, allPlayers[i].score, allPlayers[i].isLocal);
        }
    }

    void SetResultMessage(int rank)
    {
        if (resultMessageText == null) return;

        if (rank == 1) resultMessageText.text = "MÜKEMMEL OYUN!";
        else if (rank == 2) resultMessageText.text = "HARÝKA!";
        else if (rank == 3) resultMessageText.text = "ÝYÝ DENEMEYDÝ!";
        else resultMessageText.text = ""; // 4, 5, 6 için boþ býrakýr
    }

    void CreateRow(int rank, string pName, int score, bool isLocalPlayer)
    {
        GameObject row = Instantiate(playerRowPrefab, listContainer);

        row.transform.Find("Rank").GetComponent<TextMeshProUGUI>().text = rank.ToString();
        row.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = pName;
        row.transform.Find("TotalHit").GetComponent<TextMeshProUGUI>().text = score.ToString();

        if (row.transform.Find("Avatar").TryGetComponent<Image>(out var avatarImg))
            avatarImg.sprite = defaultAvatar;

        if (isLocalPlayer)
        {
            if (row.TryGetComponent<Image>(out var rowBg))
                rowBg.color = new Color(0.1f, 0.4f, 0.8f, 0.8f);
        }
    }
}
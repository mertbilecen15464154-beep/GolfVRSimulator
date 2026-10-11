using UnityEngine;
using System.Collections;

public class GolfBot : MonoBehaviour
{
    private Rigidbody rb;
    private BotVurusYolu benimYolum;
    private int vurusSirasi = 0;
    private bool isFinished = false;
    private bool canStart = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        BotVurusYolu[] tumYollar = Object.FindObjectsByType<BotVurusYolu>(FindObjectsSortMode.None);
        float enYakinMesafe = Mathf.Infinity;

        foreach (var yol in tumYollar)
        {
            float mesafe = Vector3.Distance(transform.position, yol.transform.position);
            if (mesafe < enYakinMesafe) { enYakinMesafe = mesafe; benimYolum = yol; }
        }

        StartCoroutine(BotBrain());
    }

    public void ActivateBot() => canStart = true;

    IEnumerator BotBrain()
    {
        while (!canStart) yield return new WaitForSeconds(0.1f);

        while (!isFinished && benimYolum != null)
        {
            // Topun tamamen durmasýný bekle (Doðallýk için 0.05f ideal bir eþik)
            if (rb.linearVelocity.magnitude < 0.05f && vurusSirasi < benimYolum.vuruslar.Count)
            {
                yield return new WaitForSeconds(Random.Range(1.0f, 2.0f)); // Düþünme süresi

                if (rb.linearVelocity.magnitude < 0.05f && !isFinished)
                {
                    PerformNaturalShot();
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void PerformNaturalShot()
    {
        if (vurusSirasi >= benimYolum.vuruslar.Count) return;

        VurusVerisi veri = benimYolum.vuruslar[vurusSirasi];

        // Topu vuruþ objesinin olduðu yere ýþýnla (Vuruþ hazýrlýðý)
        Vector3 startPos = veri.vurusObjesi.position;
        startPos.y += 0.05f;
        transform.position = startPos;

        // Fiziksel deðerleri sýfýrla ki önceki vuruþtan enerji kalmasýn
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Vuruþ efekti/manager tetikleyici (Varsa)
        if (TryGetComponent<BallResetManager>(out var resetManager))
            resetManager.OnClubHit();

        // SADECE VUR VE BIRAK (Hiçbir Coroutine/Düzeltme baþlatýlmaz)
        Vector3 direction = veri.vurusObjesi.forward;
        rb.AddForce(direction * veri.vurusGucu, ForceMode.Impulse);

        vurusSirasi++;
    }

    public void Finish() => isFinished = true;
}
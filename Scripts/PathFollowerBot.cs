using UnityEngine;
using System.Collections;

public class PathFollowerBot : MonoBehaviour
{
    private Rigidbody rb;
    private BotPath currentPath;
    private int currentPointIndex = 0;
    private bool canStart = false;
    private bool isFinished = false;

    [Header("Vuruþ Ayarlarý")]
    public float forceMultiplier = 5f;
    public float maxForce = 22f;
    public float minForce = 3f;
    public float stopThreshold = 0.08f;

    [Header("Zeka & Düzeltme")]
    public float pointReachedDistance = 0.5f; // Bu mesafeye gelince "ulaþtý" say ve sonrakine odaklan
    public float snapToPointForce = 2f; // Sapmalarý önlemek için hafif düzeltme gücü
    [Range(0, 5)] public float luckFactor = 1f; // Botun hata yapma payý (0 = mükemmel, 5 = çok sapar)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentPath = Object.FindFirstObjectByType<BotPath>();
        StartCoroutine(BotLogic());
    }

    public void ActivateBot() => canStart = true;

    IEnumerator BotLogic()
    {
        while (!canStart) yield return null;

        while (!isFinished && currentPath != null)
        {
            // 1. KONTROL: Eðer bot þu anki hedefine zaten çok yakýnsa, hedefi bir sonrakine kaydýr
            CheckPointProximity();

            if (rb.linearVelocity.magnitude < stopThreshold && currentPointIndex < currentPath.shotPoints.Count)
            {
                yield return new WaitForSeconds(Random.Range(0.8f, 2.0f));

                if (rb.linearVelocity.magnitude < stopThreshold)
                {
                    ShootToNextPoint();
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    void CheckPointProximity()
    {
        if (currentPointIndex >= currentPath.shotPoints.Count) return;

        float dist = Vector3.Distance(transform.position, currentPath.shotPoints[currentPointIndex].position);

        // Eðer hedef noktaya çok yakýnsak (pointReachedDistance), bir sonraki noktaya geç
        if (dist < pointReachedDistance)
        {
            Debug.Log(gameObject.name + " hedefe yakýn, hedef güncellendi.");
            currentPointIndex++;
        }
    }

    void ShootToNextPoint()
    {
        if (currentPointIndex >= currentPath.shotPoints.Count) return;

        Transform target = currentPath.shotPoints[currentPointIndex];

        // --- AKILLI HÝZALAMA (Sapma Düzeltme) ---
        // Bot tam noktada olmayabilir. Vuruþ yönünü mevcut konumdan deðil, 
        // noktaya olan açýya göre ince ayar yaparak hesaplýyoruz.
        Vector3 direction = (target.position - transform.position);
        float distance = direction.magnitude;

        // --- ÖNERÝ: HATA PAYI (Luck Factor) ---
        // Botun her zaman lazer gibi vurmasý doðal durmaz. Biraz sapma ekleyelim.
        Vector3 randomness = new Vector3(
            Random.Range(-luckFactor, luckFactor),
            0,
            Random.Range(-luckFactor, luckFactor)
        ) * (distance * 0.1f);

        Vector3 finalDirection = (direction + randomness).normalized;
        float finalForce = Mathf.Clamp(distance * forceMultiplier, minForce, maxForce);

        // Vuruþ anýnda Reset Sistemi kaydý (Daha önce eklemiþtik)
        if (TryGetComponent<BallResetManager>(out var resetManager)) resetManager.OnClubHit();

        rb.AddForce(finalDirection * finalForce, ForceMode.Impulse);

        // Vuruþtan sonra hedefi bir sonrakine hazýrla
        currentPointIndex++;
    }

    public void Finish() => isFinished = true;
}
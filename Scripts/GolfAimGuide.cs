using UnityEngine;

public class GolfAimGuide : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform clubHead;
    public Transform ball;

    [Header("Ayarlar")]
    public float aktivasyonMesafesi = 1.0f;
    public int noktaSayisi = 30;
    public float noktaAraligi = 0.1f;
    public LayerMask engelLayer; // Duvarlarý bu layer'a atamalýsýn

    [Header("Güç Ayarlarý")]
    public float minVurusGucu = 2f;
    public float maxVurusGucu = 20f;

    [Header("Hacimsel Ayarlar")]
    public float topYaricapi = 0.05f; // Topun gerçek yarýçapý (Genelde 0.05f)
    public float epsilon = 0.05f;

    public Vector3 VurusYonu => clubHead.forward;

    void Update()
    {
        if (clubHead == null || ball == null || !clubHead.gameObject.activeInHierarchy)
        {
            lineRenderer.enabled = false;
            return;
        }

        float mesafe = Vector3.Distance(clubHead.position, ball.position);

        Vector3 kafaTopYonu = (ball.position - clubHead.position).normalized;
        float bakiAcisi = Vector3.Dot(VurusYonu, kafaTopYonu);

        bool mesafeUygun = mesafe <= aktivasyonMesafesi;
        bool yonUygun = bakiAcisi > 0.5f;

        if (mesafeUygun && yonUygun)
        {
            lineRenderer.enabled = true;
            float t = 1 - (mesafe / aktivasyonMesafesi);
            float dinamikVurusGucu = Mathf.Lerp(minVurusGucu, maxVurusGucu, t);

            DrawPathWithReflections(dinamikVurusGucu);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    void DrawPathWithReflections(float guc)
    {
        lineRenderer.positionCount = noktaSayisi;
        Vector3 mevcutPozisyon = ball.position;

        Vector3 yon = VurusYonu;
        yon.y = 0;
        yon.Normalize();

        Vector3 mevcutHiz = yon * guc;
        Vector3 yercekimi = Physics.gravity;

        for (int i = 0; i < noktaSayisi; i++)
        {
            lineRenderer.SetPosition(i, mevcutPozisyon);

            float t = noktaAraligi;
            Vector3 sonrakiPozisyon = mevcutPozisyon + (mevcutHiz * t) + (0.5f * t * t * yercekimi);

            Vector3 atisYonu = (sonrakiPozisyon - mevcutPozisyon);
            float mesafe = atisYonu.magnitude;

            // SphereCast: Topun hacmiyle bir sonraki adýmý kontrol et
            if (Physics.SphereCast(mevcutPozisyon, topYaricapi, atisYonu.normalized, out RaycastHit hit, mesafe, engelLayer))
            {
                // 1. Topun duvara deðdiði an merkezinin nerede olduðunu bul
                mevcutPozisyon = mevcutPozisyon + atisYonu.normalized * hit.distance;

                // 2. Yansýma yönünü belirle
                Vector3 yeniYon = Vector3.Reflect(mevcutHiz.normalized, hit.normal);

                // 3. Mevcut hýzý yeni yöne aktar (Hýzýn büyüklüðünü koru)
                float hizBuyuklugu = mevcutHiz.magnitude;
                mevcutHiz = yeniYon * hizBuyuklugu;

                // 4. Epsilon (Kaydýrma) payýný çok küçük tut
                mevcutPozisyon += hit.normal * epsilon;
            }
            else
            {
                mevcutPozisyon = sonrakiPozisyon;
                mevcutHiz += yercekimi * t;
            }
        }
    }
}
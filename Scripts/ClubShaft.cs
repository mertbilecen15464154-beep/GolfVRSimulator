using UnityEngine;

public class ClubShaft : MonoBehaviour
{
    public enum ScaleAxis { X, Y, Z }

    [Header("Referanslar")]
    public Transform handController;
    public Transform golfBall;
    public Transform clubRoot;
    public Transform clubHoldPoint;
    public Transform clubHead;       // Sopa baþlýðý (Zemin kontrolü için)

    [Header("Yükseklik & Ölçek Ayarlarý")]
    public ScaleAxis axisToScale = ScaleAxis.Y;
    public float maxVerticalDistance = 0.9f;
    public float minVerticalDistance = 0.4f;
    public float minScale = 0.5f;
    public float maxScale = 1.0f;

    [Header("Zemin Kýsýtlamasý")]
    public float yOffset = -0.01f; // Topun merkezine göre duracaðý yükseklik

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
        if (clubRoot == null) clubRoot = transform.parent;
    }

    void LateUpdate()
    {
        if (handController == null || golfBall == null || clubHoldPoint == null || clubHead == null) return;

        // 1. ÖNCE ÖLÇEÐÝ (SCALE) HESAPLA VE UYGULA
        float currentYDist = handController.position.y - golfBall.position.y;
        float t = Mathf.InverseLerp(minVerticalDistance, maxVerticalDistance, currentYDist);
        float finalScaleFactor = Mathf.Lerp(minScale, maxScale, t);

        Vector3 newScale = transform.localScale;
        switch (axisToScale)
        {
            case ScaleAxis.X: newScale.x = initialScale.x * finalScaleFactor; break;
            case ScaleAxis.Y: newScale.y = initialScale.y * finalScaleFactor; break;
            case ScaleAxis.Z: newScale.z = initialScale.z * finalScaleFactor; break;
        }
        transform.localScale = newScale;

        // 2. SOPAYI KONTROLCÜYE YAPIÞTIR
        Vector3 separationOffset = handController.position - clubHoldPoint.position;
        clubRoot.position += separationOffset;

        // 3. KRÝTÝK DÜZELTME: ZEMÝN / TOP KONTROLÜ
        // Eðer sopa kafasý topun belirlenen seviyesinin altýna inerse, tüm sopayý yukarý it
        float minY = golfBall.position.y - yOffset;
        if (clubHead.position.y < minY)
        {
            float pushUpAmount = minY - clubHead.position.y;
            clubRoot.position += new Vector3(0, pushUpAmount, 0);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class VurusVerisi
{
    public Transform vurusObjesi;
    public float vurusGucu = 10f;
    public bool otomatikGuc = true;
    public bool ziplamaNoktasi = false;
    public bool engelVar = false;
    [Range(0, 10)] public int maxSekmeSayisi = 1;
}

public class BotVurusYolu : MonoBehaviour
{
    public List<VurusVerisi> vuruslar = new();

    [Header("Matematiksel Çarpanlar")]
    public float GUC_CARPANI = 2.771f;
    public float ZIPLAMA_ORANI = 0.4292227f;

    [Header("Engel Ayarlarý")]
    public LayerMask engelLayer;

    void Update()
    {
        AutoUpdateForcesAndPositions();
    }

    private void AutoUpdateForcesAndPositions()
    {
        for (int i = 0; i < vuruslar.Count - 1; i++)
        {
            if (vuruslar[i].vurusObjesi == null || vuruslar[i + 1].vurusObjesi == null) continue;

            Vector3 currentOrigin = vuruslar[i].vurusObjesi.position;
            Vector3 currentDirection = vuruslar[i].vurusObjesi.forward;
            float toplamMesafe = 0;

            int sekmeSayisi = 0;
            Vector3 sonNokta = currentOrigin;

            // --- SEKMELERÝ HESAPLA ---
            while (sekmeSayisi < vuruslar[i].maxSekmeSayisi)
            {
                if (Physics.Raycast(sonNokta, currentDirection, out RaycastHit hit, 100f, engelLayer))
                {
                    toplamMesafe += hit.distance;
                    sonNokta = hit.point;
                    currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                    sekmeSayisi++;
                }
                else break;
            }

            // --- DOÐAL GÜÇ HESABI ---
            // Sadece mesafeyi ölçüyoruz, objeyi (vuruslar[i+1]) bir yere taþýmýyoruz.
            float kalanMesafe = Vector3.Distance(sonNokta, vuruslar[i + 1].vurusObjesi.position);
            toplamMesafe += kalanMesafe;

            if (vuruslar[i].otomatikGuc)
            {
                float hesaplananGuc = toplamMesafe * GUC_CARPANI;
                vuruslar[i].vurusGucu = vuruslar[i].ziplamaNoktasi ? hesaplananGuc * ZIPLAMA_ORANI : hesaplananGuc;
            }
        }
    }

    public void YeniVurusYoluOlustur()
    {
        int yeniNumara = vuruslar.Count + 1;
        GameObject yeniObj = new($"VurusYolu ({yeniNumara})");

        if (vuruslar.Count > 0 && vuruslar[^1].vurusObjesi != null)
        {
            Transform oncekiTransform = vuruslar[^1].vurusObjesi;
            yeniObj.transform.SetParent(this.transform); // Parent artýk ana obje olsun, zincirleme gitmesin
            yeniObj.transform.position = oncekiTransform.position + (oncekiTransform.forward * 3f);
            yeniObj.transform.rotation = oncekiTransform.rotation;
        }
        else
        {
            yeniObj.transform.SetParent(this.transform);
            yeniObj.transform.position = transform.position;
        }

        vuruslar.Add(new VurusVerisi { vurusObjesi = yeniObj.transform, otomatikGuc = true });
    }

    private void OnDrawGizmos()
    {
        if (vuruslar == null || vuruslar.Count == 0) return;
        if (!Application.isPlaying) AutoUpdateForcesAndPositions();

        for (int i = 0; i < vuruslar.Count; i++)
        {
            if (vuruslar[i].vurusObjesi == null) continue;

            Gizmos.color = vuruslar[i].ziplamaNoktasi ? Color.magenta : Color.cyan;
            Gizmos.DrawWireSphere(vuruslar[i].vurusObjesi.position, 0.04f);

            if (i < vuruslar.Count - 1 && vuruslar[i + 1].vurusObjesi != null)
            {
                Vector3 pos = vuruslar[i].vurusObjesi.position;
                Vector3 dir = vuruslar[i].vurusObjesi.forward;
                Gizmos.color = vuruslar[i].ziplamaNoktasi ? Color.magenta : Color.yellow;

                int currentSekme = 0;
                Vector3 drawingPos = pos;

                while (currentSekme < vuruslar[i].maxSekmeSayisi)
                {
                    if (Physics.Raycast(drawingPos, dir, out RaycastHit hit, 100f, engelLayer))
                    {
                        Gizmos.DrawLine(drawingPos, hit.point);
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireSphere(hit.point, 0.06f);
                        Gizmos.color = vuruslar[i].ziplamaNoktasi ? Color.magenta : Color.yellow;

                        drawingPos = hit.point;
                        dir = Vector3.Reflect(dir, hit.normal);
                        currentSekme++;
                    }
                    else break;
                }
                Gizmos.DrawLine(drawingPos, vuruslar[i + 1].vurusObjesi.position);
            }
        }
    }
}
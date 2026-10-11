using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jumper : MonoBehaviour
{
    [Header("Zýplama Ayarlarý")]
    public float temelZiplamaKuvveti = 5f;
    public float hizEtkiCarpani = 0.5f;
    [Range(0.01f, 1.0f)]
    public float mikroIleriItme = 0.5f;

    [Header("Zaman Ayarlarý")]
    public float beklemeSuresi = 0.3f;

    private readonly Dictionary<Rigidbody, bool> ziplamaSozlugu = new Dictionary<Rigidbody, bool>();

    private void OnTriggerEnter(Collider other)
    {
        CheckAndJump(other);
    }

    private void OnTriggerStay(Collider other)
    {
        CheckAndJump(other);
    }

    private void CheckAndJump(Collider other)
    {
        // KONTROL: Objenin adýnda "GolfBall" geçiyor mu? 
        // Bu hem "GolfBall" (Senin topun) hem de "GolfBall_Bot" (Botlar) için çalýþýr.
        if (other.name.Contains("GolfBall"))
        {
            Rigidbody rb = other.attachedRigidbody;

            if (rb != null)
            {
                if (!ziplamaSozlugu.ContainsKey(rb))
                    ziplamaSozlugu.Add(rb, true);

                // Zýplama kilidi açýk mý ve top hareket ediyor mu?
                if (ziplamaSozlugu[rb] && rb.linearVelocity.magnitude > 0.001f)
                {
                    Ziplat(rb);
                }
            }
        }
    }

    private void Ziplat(Rigidbody rb)
    {
        ziplamaSozlugu[rb] = false;

        // 1. Mevcut hýzý ve yönü al
        Vector3 v = rb.linearVelocity;
        Vector3 ileriYon = new Vector3(v.x, 0, v.z).normalized;

        // Eðer top çok yavaþsa ve yön kestirilemiyorsa Jumper'ýn baktýðý yöne fýrlat
        if (ileriYon == Vector3.zero) ileriYon = transform.forward;

        // 2. Dikey hýzý sýfýrla (Üst üste binmeleri önle)
        rb.linearVelocity = new Vector3(v.x, 0, v.z);

        // 3. Dinamik güç hesabý
        float yatayHiz = new Vector3(v.x, 0, v.z).magnitude;
        float dinamikKuvvet = temelZiplamaKuvveti + (yatayHiz * hizEtkiCarpani);

        // 4. Kuvvet Uygula
        Vector3 firlatmaKuvveti = (Vector3.up * dinamikKuvvet) + (dinamikKuvvet * mikroIleriItme * ileriYon);
        rb.AddForce(firlatmaKuvveti, ForceMode.Impulse);

        Debug.Log($"<color=orange>Jumper:</color> {rb.gameObject.name} (Zýplama Gücü: {dinamikKuvvet:F2})");

        StartCoroutine(ZiplamaKilidiniAc(rb));
    }

    private IEnumerator ZiplamaKilidiniAc(Rigidbody rb)
    {
        yield return new WaitForSeconds(beklemeSuresi);
        if (rb != null && ziplamaSozlugu.ContainsKey(rb))
        {
            ziplamaSozlugu[rb] = true;
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class WaterCurrent : MonoBehaviour
{
    [Header("Akýntý Yönü (Sadece X ve Z)")]
    public float currentDirectionX = 1f;
    public float currentDirectionZ = 0f;

    [Header("Akýntý Ayarlarý")]
    public float currentStrength = 3f; // Akýntýnýn gücü

    [Header("Su Direnci (Damping)")]
    [Tooltip("Top sudayken hýzý ne kadar sönümlensin? (0.95 gibi bir deðer suyun içinde topun kontrolsüzce hýzlanmasýný engeller)")]
    public float waterDrag = 0.95f;

    [Header("Ses Ayarlarý")]
    public AudioSource audioSource;
    public AudioClip waterSplashSound; // Suya düþme sesi

    private readonly List<Rigidbody> ballsInWater = new();

    private void FixedUpdate()
    {
        Vector3 pushVector = new Vector3(currentDirectionX, 0, currentDirectionZ).normalized * currentStrength;

        for (int i = ballsInWater.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = ballsInWater[i];

            if (rb != null)
            {
                // 1. Akýntý kuvvetini uygula (Sürekli sabit itiþ)
                rb.AddForce(pushVector, ForceMode.Acceleration);

                // 2. Su direnci uygula (Topun su içinde sonsuza kadar hýzlanmamasý için)
                rb.linearVelocity *= waterDrag;
            }
            else
            {
                ballsInWater.RemoveAt(i);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Tag ve Rigidbody kontrolü
        if (other.CompareTag("GolfBall") && other.attachedRigidbody != null)
        {
            Rigidbody ballRb = other.attachedRigidbody;

            // 2. Eðer top listede yoksa, SUYA YENÝ GÝRMÝÞTÝR
            if (!ballsInWater.Contains(ballRb))
            {
                ballsInWater.Add(ballRb);

                // --- SES EFEKTÝ ---
                if (audioSource != null && waterSplashSound != null)
                {
                    // Topun hýzýna göre ses þiddetini ayarla (hýzlý düþerse çok ses çýkar)
                    float impactSpeed = ballRb.linearVelocity.magnitude;
                    float volume = Mathf.Clamp(impactSpeed / 5f, 0.2f, 1.0f);

                    audioSource.PlayOneShot(waterSplashSound, volume);
                }

                // Görsel efekt istersen (Particle) tam burada spawn edebilirsin:
                // Instantiate(splashParticle, other.transform.position, Quaternion.identity);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null && ballsInWater.Contains(other.attachedRigidbody))
        {
            ballsInWater.Remove(other.attachedRigidbody);
        }
    }

    // Editörde akýntýnýn yönünü okla gösterir
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 dir = new Vector3(currentDirectionX, 0, currentDirectionZ).normalized;
        Gizmos.DrawRay(transform.position, dir * 2f);
        Gizmos.DrawSphere(transform.position + dir * 2f, 0.2f);
    }
}
using UnityEngine;

public class SlipperySurface : MonoBehaviour
{
    [Header("Kaydýrma Gücü")]
    public float extraSlideForce = 2f;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("GolfBall"))
        {
            Rigidbody rb = collision.rigidbody;
            if (rb != null)
            {
                // Engelin o anki yüzey eðimini (normal) alýyoruz
                Vector3 surfaceNormal = collision.contacts[0].normal;

                // Eðer yüzey tam dik deðilse (eðimliyse), aþaðý yönlü bir kuvvet uygula
                if (surfaceNormal.y < 0.99f)
                {
                    // Eðim yönünü hesapla (Yatay düzlemde aþaðý doðru)
                    Vector3 slideDirection = new(surfaceNormal.x, -0.5f, surfaceNormal.z);
                    rb.AddForce(slideDirection * extraSlideForce, ForceMode.Acceleration);
                }
            }
        }
    }
}
using UnityEngine;
using System.Collections;

public class MovingObstacle : MonoBehaviour
{
    [Header("Rotasyon Ayarlarý")]
    public Vector3 minRotation; // Senin durumunda 90 derece -> KAPI DÝK (Collider AÇIK)
    public Vector3 maxRotation; // Senin durumunda 0 derece -> KAPI YATIK (Collider KAPALI)
    public float transitionSpeed = 2f;
    public float waitTime = 2f;

    [Header("Referanslar")]
    public BoxCollider obstacleCollider;

    private void Start()
    {
        // Baþlangýçta kapýyý dik konuma getir (minRotation) ve collider'ý AÇIK yap
        transform.localRotation = Quaternion.Euler(minRotation);
        if (obstacleCollider != null) obstacleCollider.enabled = true;

        StartCoroutine(RotationCycle());
    }

    IEnumerator RotationCycle()
    {
        while (true)
        {
            // 1. KAPI YATIYOR (maxRotation - 0 dereceye)
            yield return RotateRoutine(maxRotation);

            // Tamamen yattýðýnda collider'ý kapat
            if (obstacleCollider != null) obstacleCollider.enabled = false;

            yield return new WaitForSeconds(waitTime);

            // 2. KAPI KALKIYOR (minRotation - 90 dereceye)
            // Kalkmaya baþladýðý an collider'ý aç
            if (obstacleCollider != null) obstacleCollider.enabled = true;

            yield return RotateRoutine(minRotation);

            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator RotateRoutine(Vector3 targetEuler)
    {
        Quaternion targetRotation = Quaternion.Euler(targetEuler);

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * transitionSpeed
            );
            yield return null;
        }
        transform.localRotation = targetRotation;
    }
}
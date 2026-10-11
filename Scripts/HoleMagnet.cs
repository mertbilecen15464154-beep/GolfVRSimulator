using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HoleMagnet : MonoBehaviour
{
    [Header("Mýknatýs Ayarlarý")]
    public float strength = 15f;
    public float radius = 0.5f;
    public AnimationCurve forceDistanceCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Efektler & Konfeti")]
    public AudioSource audioSource;
    public AudioClip holeInSound;
    public GameObject confettiObject;
    public float confettiDuration = 3.0f;

    [Header("Referanslar")]
    public LevelEndManager levelEndManager;

    private readonly List<Rigidbody> affectedBalls = new();
    private bool playerHasFinished = false;

    private void FixedUpdate()
    {
        for (int i = affectedBalls.Count - 1; i >= 0; i--)
        {
            Rigidbody ball = affectedBalls[i];
            if (ball == null) { affectedBalls.RemoveAt(i); continue; }

            Vector3 directionToCenter = transform.position - ball.transform.position;
            directionToCenter.y = 0; // Sadece yatayda çekmeye devam etsin
            float distance = directionToCenter.magnitude;

            if (distance <= radius)
            {
                float distanceValue = Mathf.Clamp01(distance / radius);
                float curveEval = forceDistanceCurve.Evaluate(1f - distanceValue);
                float finalForce = curveEval * strength;

                ball.AddForce(directionToCenter.normalized * finalForce, ForceMode.Acceleration);
            }
        }
    }

    // --- YENÝ TETÝKLEYÝCÝ MANTIK ---
    // Deliðin içine koyduðun küçük trigger collider buna çarpacak
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GolfBall") && other.attachedRigidbody != null)
        {
            Rigidbody ballRb = other.attachedRigidbody;

            // 1. Eðer mýknatýs alanýna girdiyse listeye ekle
            if (!affectedBalls.Contains(ballRb))
                affectedBalls.Add(ballRb);

            // 2. Eðer bu çarpýþma deliðin tam ÝÇÝNDEKÝ küçük collider ise (IsTrigger)
            // Not: Hem mýknatýs alaný hem iç trigger ayný scripti kullanýyorsa 
            // derinlik kontrolü için bir CheckBallEntry tetikleyelim.

            // Eðer top deliðin içine tam olarak girdiyse (Yatay mesafe çok azsa)
            float hDist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                         new Vector3(other.transform.position.x, 0, other.transform.position.z));

            // Küçük trigger'a çarptýðý an vuruþu bitir
            if (hDist < 0.1f) // Mýknatýsýn dýþ çemberiyle karýþmamasý için küçük bir güvenlik kontrolü
            {
                CheckBallEntry(ballRb);
            }
        }
    }

    void CheckBallEntry(Rigidbody ball)
    {
        // Listeden çýkar ki çekmeye devam etmesin
        if (affectedBalls.Contains(ball)) affectedBalls.Remove(ball);

        if (ball.name.Contains("GolfBall_Bot"))
        {
            StartCoroutine(BotFinishSequence(ball));
            if (levelEndManager != null) levelEndManager.PlayerFinished();
        }
        else if (!playerHasFinished)
        {
            StartCoroutine(PlayerCelebrationSequence(ball));
            if (levelEndManager != null) levelEndManager.PlayerFinished();
        }
    }

    IEnumerator BotFinishSequence(Rigidbody ball)
    {
        if (ball.TryGetComponent<GolfBot>(out var botScript)) botScript.Finish();
        ball.linearDamping = 10f;
        ball.gameObject.SetActive(false); // Anýnda gizle
        yield break;
    }

    IEnumerator PlayerCelebrationSequence(Rigidbody ball)
    {
        playerHasFinished = true;

        if (audioSource != null && holeInSound != null)
            audioSource.PlayOneShot(holeInSound);

        if (confettiObject != null)
        {
            confettiObject.SetActive(true);
            if (confettiObject.TryGetComponent<ParticleSystem>(out var ps)) ps.Play();
        }

        if (levelEndManager != null) levelEndManager.FinishLevel();

        ball.linearDamping = 10f;
        ball.gameObject.SetActive(false);

        yield return new WaitForSeconds(confettiDuration);
        if (confettiObject != null) confettiObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null && affectedBalls.Contains(other.attachedRigidbody))
            affectedBalls.Remove(other.attachedRigidbody);
    }

    private void OnEnable() => playerHasFinished = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
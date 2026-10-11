using UnityEngine;
using System.Collections;

public class BallResetManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public float movementThreshold = 0.05f;

    private GolfBallPhysics golfBallPhysics;
    private Vector3 lastSafePosition;
    private Vector3 positionAtHit;
    private Rigidbody rb;
    private bool isResetting = false;
    private bool waitingForMovement = false;

    [Header("Ses Ayarlarý")]
    public AudioSource outOfBoundsSoundAudioSource, obstacleSoundAudioSource;
    public AudioClip outOfBoundsSound, obstacleSound;

    // Sopa kilidi. Doðru vuruþ için bu True olmalý.
    private bool canBeHit = true;
    private float originalGravity;

    void Start()
    {
        golfBallPhysics = GetComponent<GolfBallPhysics>();
        rb = GetComponent<Rigidbody>();
        lastSafePosition = transform.position;

        if (golfBallPhysics != null)
            originalGravity = golfBallPhysics.gravityMultiplier;
    }

    // Sopa deðdiðinde çaðrýlan ana fonksiyon
    public void OnClubHit()
    {
        // Eðer top zaten hareket halindeyse veya kilitliyse yeni vuruþu reddet
        if (!canBeHit || isResetting || waitingForMovement) return;

        canBeHit = false; // KÝLÝDÝ KAPAT: Sopa artýk bu topa dokunsa da iþlem yapamaz.
        positionAtHit = transform.position;
        waitingForMovement = true;
    }

    void Update()
    {
        if (waitingForMovement)
        {
            if (Vector3.Distance(transform.position, positionAtHit) >= movementThreshold)
            {
                ConfirmStroke();
            }
        }

        // TOP DURDUÐUNDA KÝLÝDÝ AÇ
        // Hýz çok düþtüðünde ve artýk hareket beklemiyorken sopaya tekrar izin ver
        if (!waitingForMovement && !canBeHit && rb.linearVelocity.magnitude < 0.05f)
        {
            canBeHit = true;
        }

        // Eðer top ýþýnlanmýþsa ve yere çarpmýþsa yerçekimini eski haline çek
        if (golfBallPhysics.gravityMultiplier > originalGravity && golfBallPhysics.IsGrounded())
        {
            golfBallPhysics.gravityMultiplier = originalGravity;
        }
    }

    private void ConfirmStroke()
    {
        waitingForMovement = false;
        if (ScoreManager.instance != null) ScoreManager.instance.AddStroke();
        lastSafePosition = positionAtHit;
    }

    public void ResetToLastPosition()
    {
        if (isResetting) return;
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        isResetting = true;
        waitingForMovement = false;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        yield return new WaitForSeconds(0.1f);
        transform.position = lastSafePosition;
        Physics.SyncTransforms();

        rb.isKinematic = false;

        // TOP IÞINLANDIÐINDA YERÇEKÝMÝNÝ 50 YAP (Hýzlý düþmesi için)
        if (golfBallPhysics != null)
            golfBallPhysics.gravityMultiplier = 50f;

        yield return new WaitForSeconds(0.3f);

        isResetting = false;
        canBeHit = true; // Resetlenince sopaya tekrar izin ver
    }

    public bool IsResettingNow()
    {
        return isResetting;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OutOfBounds") && !isResetting)
        {
            ResetToLastPosition();
            // --- SES EFEKTÝ ---
            if (outOfBoundsSoundAudioSource != null && outOfBoundsSound != null)
            {
                float volume = Mathf.Clamp(5f, 0.2f, 1.0f);
                outOfBoundsSoundAudioSource.PlayOneShot(outOfBoundsSound, volume);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // --- SES EFEKTÝ ---
            if (obstacleSoundAudioSource != null && obstacleSound != null)
            {
                float volume = Mathf.Clamp(5f, 0.2f, 1.0f);
                obstacleSoundAudioSource.PlayOneShot(obstacleSound, volume);
            }
        }
    }
}
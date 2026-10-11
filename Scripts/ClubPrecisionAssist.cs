using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ClubPrecisionAssist : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform clubHeadTransform;
    public GolfAimGuide aimGuide;

    [Header("Vuruþ Ayarlarý")]
    public float hitPowerMultiplier = 1.6f;
    public float minVelocityThreshold = 0.1f;
    public float hitCooldown = 0.5f;

    [Header("Ses Ayarlarý")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public float minHitForce = 0.5f;

    [Header("Precision Mode (Trigger)")]
    public InputActionReference triggerAction;

    private float lastHitSoundTime;
    private bool isAimLocked = false;
    private bool needsTriggerRelease = false;
    private Vector3 lockedDirection = Vector3.zero;

    private Vector3 lastHeadPosition;
    private Vector3 currentHeadVelocity;
    private float lastHitTime;
    private Rigidbody clubRb;

    void Start()
    {
        clubRb = GetComponent<Rigidbody>();
        if (clubHeadTransform != null) lastHeadPosition = clubHeadTransform.position;
        SetupHighPrecisionPhysics();
    }

    void SetupHighPrecisionPhysics()
    {
        if (clubRb != null)
        {
            clubRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            clubRb.solverIterations = 255;
            clubRb.isKinematic = true;
        }
    }

    void Update()
    {
        HandlePrecisionMode();

        if (clubHeadTransform == null) return;
        currentHeadVelocity = (clubHeadTransform.position - lastHeadPosition) / Time.deltaTime;
        lastHeadPosition = clubHeadTransform.position;
    }

    void HandlePrecisionMode()
    {
        // SÜRE KONTROLÜ: Süre bittiyse niþangahý kapat ve kilitlenmeye izin verme
        if (TimerManager.instance != null && TimerManager.instance.IsTimeOver())
        {
            isAimLocked = false;
            if (aimGuide != null)
            {
                aimGuide.enabled = false;
                if (aimGuide.lineRenderer != null) aimGuide.lineRenderer.enabled = false;
            }
            return;
        }

        if (triggerAction == null) return;

        float triggerValue = triggerAction.action.ReadValue<float>();

        if (triggerValue < 0.5f)
        {
            isAimLocked = false;
            needsTriggerRelease = false;
            lockedDirection = Vector3.zero;

            if (aimGuide != null) aimGuide.enabled = true;
            return;
        }

        if (needsTriggerRelease)
        {
            if (aimGuide != null)
            {
                aimGuide.enabled = false;
                if (aimGuide.lineRenderer != null) aimGuide.lineRenderer.enabled = false;
            }
            return;
        }

        if (triggerValue > 0.5f && !isAimLocked)
        {
            lockedDirection = clubHeadTransform.forward;
            lockedDirection.y = 0;
            lockedDirection.Normalize();

            isAimLocked = true;
            if (aimGuide != null) aimGuide.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // SÜRE KONTROLÜ: Süre bittiyse vuruþu fiziksel olarak da engelle
        if (TimerManager.instance != null && TimerManager.instance.IsTimeOver()) return;

        if (Time.time - lastHitTime < hitCooldown) return;

        if (other.CompareTag("GolfBall"))
        {
            HandleHit(other.gameObject);
        }
    }

    private void HandleHit(GameObject ballObj)
    {
        // SÜRE KONTROLÜ: Fonksiyonun baþýnda son bir kontrol
        if (TimerManager.instance != null && TimerManager.instance.IsTimeOver()) return;

        if (ballObj.TryGetComponent<BallResetManager>(out var resetManager))
        {
            float hitSpeed = currentHeadVelocity.magnitude;

            if (hitSpeed > minVelocityThreshold)
            {
                PlayHitSound(hitSpeed);

                // BallResetManager'a vuruþ yapýldýðýný bildir (Süre kontrolü orada da var)
                resetManager.OnClubHit();

                lastHitTime = Time.time;

                Vector3 finalVelocity;
                float power = hitSpeed * hitPowerMultiplier;

                if (isAimLocked)
                {
                    finalVelocity = lockedDirection * power;
                    isAimLocked = false;
                    needsTriggerRelease = true;
                    lockedDirection = Vector3.zero;
                }
                else
                {
                    Vector3 rawHitVelocity = currentHeadVelocity * hitPowerMultiplier;
                    finalVelocity = new Vector3(rawHitVelocity.x, 0, rawHitVelocity.z);
                }

                if (aimGuide != null)
                {
                    if (aimGuide.lineRenderer != null) aimGuide.lineRenderer.enabled = false;
                    aimGuide.enabled = false;
                }

                if (ballObj.TryGetComponent<Rigidbody>(out var ballRb))
                {
                    ballRb.isKinematic = false;
                    ballRb.WakeUp();
                    ballRb.linearVelocity = finalVelocity;
                }
            }
        }
    }

    void PlayHitSound(float speed)
    {
        if (audioSource != null && hitSound != null && Time.time - lastHitSoundTime > hitCooldown)
        {
            if (speed > minHitForce)
            {
                float dynamicVolume = Mathf.Clamp(speed / 10f, 0.1f, 1.0f);
                audioSource.PlayOneShot(hitSound, dynamicVolume);
                lastHitSoundTime = Time.time;
            }
        }
    }
}
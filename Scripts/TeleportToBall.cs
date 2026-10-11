using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportToBall : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform playerTransform;
    public Transform golfBall;
    private Rigidbody ballRb;
    private BallResetManager resetManager;

    [Header("Iþýnlanma Ayarlarý")]
    public float distanceFromBall = 1.0f;
    public float heightOffset = 0.0f;
    [Tooltip("Her adýmda kaç derece dönsün? 45 derece 8 yön yapar.")]
    public float angleStep = 15f;

    [Header("Sürekli Dönüþ Ayarlarý")]
    [Tooltip("Sürekli dönüþün baþlamasý için thumbstick ne kadar süre basýlý tutulmalý?")]
    public float continuousRotationDelay = 0.5f;
    [Tooltip("Sürekli dönüþ baþladýðýnda saniyede kaç adým atlasýn?")]
    public float rotationStepsPerSecond = 4f;

    [Header("Giriþ Ayarlarý")]
    public InputActionProperty thumbstickAction;

    private int currentStep = 0;
    private bool isBallMoving = false;
    private bool inputLocked = false;

    // Sürekli dönüþ için gereken yeni deðiþkenler
    private float holdTimer = 0f;
    private float stepTimer = 0f;

    void Start()
    {
        if (golfBall != null)
        {
            ballRb = golfBall.GetComponent<Rigidbody>();
            resetManager = golfBall.GetComponent<BallResetManager>();
        }
    }

    void Update()
    {
        if (resetManager != null && resetManager.IsResettingNow()) return;

        CheckBallStopping();
        HandleInput();
    }

    void CheckBallStopping()
    {
        if (ballRb == null) return;
        float speed = ballRb.linearVelocity.magnitude;
        if (speed > 0.1f && !isBallMoving) isBallMoving = true;
        if (speed < 0.05f && isBallMoving)
        {
            isBallMoving = false;
            TeleportToStep();
        }
    }

    void HandleInput()
    {
        Vector2 stickValue = thumbstickAction.action.ReadValue<Vector2>();

        // THUMBSTICK AKTÝF (Saða veya Sola itilmiþ)
        if (Mathf.Abs(stickValue.x) > 0.7f)
        {
            int direction = stickValue.x > 0 ? 1 : -1;

            // 1. ÝLK TIK (Tek seferlik ýþýnlanma)
            if (!inputLocked)
            {
                currentStep += direction;
                TeleportToStep();
                inputLocked = true;
                holdTimer = 0f; // Zamanlayýcýyý baþlat
            }
            else
            {
                // 2. BASILI TUTMA KONTROLÜ
                holdTimer += Time.deltaTime;

                if (holdTimer >= continuousRotationDelay)
                {
                    stepTimer += Time.deltaTime;
                    float timeBetweenSteps = 1f / rotationStepsPerSecond;

                    if (stepTimer >= timeBetweenSteps)
                    {
                        currentStep += direction;
                        TeleportToStep();
                        stepTimer = 0f; // Bir sonraki adým için sýfýrla
                    }
                }
            }
        }
        else
        {
            // THUMBSTICK BIRAKILDI
            inputLocked = false;
            holdTimer = 0f;
            stepTimer = 0f;
        }
    }

    public void TeleportToStep()
    {
        if (golfBall == null) return;

        float angle = currentStep * angleStep;
        float radians = angle * Mathf.Deg2Rad;

        float x = Mathf.Sin(radians) * distanceFromBall;
        float z = -Mathf.Cos(radians) * distanceFromBall;

        Vector3 offset = new(x, 0, z);
        Vector3 targetPos = golfBall.position + offset;
        targetPos.y = golfBall.position.y + heightOffset;

        playerTransform.position = targetPos;

        Vector3 lookAtBall = golfBall.position - playerTransform.position;
        lookAtBall.y = 0;
        if (lookAtBall != Vector3.zero)
        {
            playerTransform.rotation = Quaternion.LookRotation(lookAtBall);
        }
    }
}
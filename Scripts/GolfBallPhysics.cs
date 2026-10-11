using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GolfBallPhysics : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Golf Battle Ayarlarý")]
    public float linearDragGround = 0.2f;
    public float linearDragAir = 0f;
    public float stopThreshold = 0.01f;
    public float gravityMultiplier = -2f;

    [Header("Engel Ayarlarý")]
    public float obstacleGravityMultiplier = 50f;
    private bool isTouchingObstacle = false;

    [Header("Görsel Efektler")]
    public TrailRenderer ballTrail;
    public float trailSpeedThreshold = 0f;

    [Header("Vuruþ Kontrolü")]
    public bool lockVerticalOnHit = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return;

        // Yerçekimi Uygulama
        if (isTouchingObstacle)
        {
            rb.AddForce(Vector3.down * obstacleGravityMultiplier, ForceMode.Acceleration);
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }

        // Sürtünme Kontrolü
        if (IsGrounded())
            rb.linearDamping = linearDragGround;
        else
            rb.linearDamping = linearDragAir;

        // Otomatik Durma
        // ÖNEMLÝ: rb.linearVelocity.magnitude kontrolü hem bot hem oyuncu için durmayý saðlar
        if (rb.linearVelocity.magnitude < stopThreshold && IsGrounded())
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        if (ballTrail != null)
        {
            ballTrail.emitting = rb.linearVelocity.magnitude > trailSpeedThreshold;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle")) isTouchingObstacle = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle")) isTouchingObstacle = false;
    }

    // Bu fonksiyonu artýk GolfBot.cs içinden de çaðýracaðýz!
    public void ForceHorizontalHit(Vector3 rawVelocity)
    {
        Vector3 horizontalVelocity = new(rawVelocity.x, 0f, rawVelocity.z);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(horizontalVelocity, ForceMode.Impulse);
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.25f);
    }
}
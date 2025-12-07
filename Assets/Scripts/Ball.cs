using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public float speed = 6f;
    Rigidbody2D rb;
    // maximum bounce angle (degrees) from the horizontal when hitting a paddle
    public float maxBounceAngle = 75f;
    // Enable collision debug logs when true
    public bool debugCollisions = false;
    // Minimum horizontal velocity as fraction of total speed to avoid near-vertical bounces
    public float minHorizontalRatio = 0.25f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        ResetBall(Random.value > 0.5f);
    }

    public void ResetBall(bool toRight)
    {
        transform.position = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        float angle = Random.Range(-0.3f, 0.3f); // slight vertical variation
        Vector2 dir = new Vector2(toRight ? 1f : -1f, angle).normalized;
        rb.linearVelocity = dir * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // unified collision handling
        if (collision.contactCount == 0) return;
        var contact = collision.GetContact(0);

        // Paddle hit: compute outgoing angle based on hit position
        if (collision.gameObject.CompareTag("Paddle"))
        {
            if (debugCollisions) Debug.Log($"Ball hit Paddle at point {contact.point}, paddle {collision.transform.name}");
            var paddle = collision.transform;
            var paddleCollider = collision.collider as Collider2D;

            float paddleHeight = 1f;
            if (paddleCollider != null) paddleHeight = paddleCollider.bounds.size.y;
            float relativeY = (contact.point.y - paddle.position.y) / (paddleHeight / 2f);
            relativeY = Mathf.Clamp(relativeY, -1f, 1f);

            float bounceAngle = relativeY * maxBounceAngle * Mathf.Deg2Rad;
            float dirX = (paddle.position.x < 0f) ? 1f : -1f;
            Vector2 outDir = new Vector2(Mathf.Cos(bounceAngle) * dirX, Mathf.Sin(bounceAngle)).normalized;

            float inSpeed = rb.linearVelocity.magnitude;
            float newSpeed = Mathf.Max(inSpeed * 1.05f, speed * 0.5f);

            Vector2 proposed = outDir * newSpeed;
            float minHx = Mathf.Abs(minHorizontalRatio * newSpeed);
            float vx = proposed.x;
            float vy = proposed.y;
            if (Mathf.Abs(vx) < minHx)
            {
                float signX = (vx != 0f) ? Mathf.Sign(vx) : (paddle.position.x < 0f ? 1f : -1f);
                vx = signX * minHx;
                float rest = Mathf.Max(0f, newSpeed * newSpeed - vx * vx);
                vy = Mathf.Sign(vy) * Mathf.Sqrt(rest);
            }
            rb.linearVelocity = new Vector2(vx, vy);
            return;
        }

        // Non-paddle: reflect by contact normal
        if (debugCollisions) Debug.Log($"Ball hit {collision.gameObject.name} normal={contact.normal} incomingVel={rb.linearVelocity}");
        Vector2 inVel = rb.linearVelocity;
        if (inVel.magnitude < 0.01f) inVel = new Vector2((Random.value > 0.5f) ? 1f : -1f, 0f) * speed;
        Vector2 reflected = Vector2.Reflect(inVel.normalized, contact.normal);
        float mag = inVel.magnitude;

        if (collision.gameObject.CompareTag("Wall"))
        {
            // Use full-velocity reflect to preserve exact incident angle and magnitude,
            // then increase speed slightly (5%) so bounces speed up.
            Vector2 reflectedFull = Vector2.Reflect(inVel, contact.normal);
            // If the reflected vertical component is effectively zero, nudge it slightly
            // so the ball will not continue perfectly horizontal after a wall hit.
            if (Mathf.Abs(reflectedFull.y) < 0.01f)
            {
                // Nudge away from the wall depending on which wall was hit
                float nudge = 0.2f * (contact.normal.y > 0f ? 1f : -1f);
                reflectedFull.y = nudge;
            }
            rb.linearVelocity = reflectedFull.normalized * (mag * 1.05f);
        }
        else
        {
            // for other objects keep safety clamp on horizontal and increase speed slightly
            mag *= 1.05f;
            Vector2 proposedRef = reflected * mag;
            float minH = Mathf.Abs(minHorizontalRatio * mag);
            float rvx = proposedRef.x;
            float rvy = proposedRef.y;
            if (Mathf.Abs(rvx) < minH)
            {
                float signX = (rvx != 0f) ? Mathf.Sign(rvx) : ((transform.position.x >= 0f) ? -1f : 1f);
                rvx = signX * minH;
                float rest = Mathf.Max(0f, mag * mag - rvx * rvx);
                rvy = Mathf.Sign(rvy) * Mathf.Sqrt(rest);
            }
            rb.linearVelocity = new Vector2(rvx, rvy);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Goals should be trigger colliders with tags "GoalLeft" and "GoalRight"
        if (other.CompareTag("GoalLeft"))
        {
            // Right player scores (index 1)
            if (GameManager.Instance != null) GameManager.Instance.ScorePoint(1);
        }
        else if (other.CompareTag("GoalRight"))
        {
            // Left player scores (index 0)
            if (GameManager.Instance != null) GameManager.Instance.ScorePoint(0);
        }
    }
}

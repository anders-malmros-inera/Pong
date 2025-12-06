using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public float speed = 8f;
    Rigidbody2D rb;

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
        // increase speed slightly on paddle hit
        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Reflect the velocity by the contact normal to ensure a proper bounce
            if (collision.contactCount > 0)
            {
                var contact = collision.GetContact(0);
                Vector2 inVel = rb.linearVelocity;
                if (inVel.magnitude < 0.01f)
                {
                    // fallback: if velocity is effectively zero, nudge away from paddle
                    inVel = new Vector2(transform.position.x < collision.transform.position.x ? -1f : 1f, 0f);
                }
                Vector2 reflected = Vector2.Reflect(inVel.normalized, contact.normal);
                float newSpeed = Mathf.Max(inVel.magnitude * 1.05f, speed * 0.5f);
                rb.linearVelocity = reflected * newSpeed;
            }
            else
            {
                // no contact info; as a fallback, just slightly increase current velocity
                rb.linearVelocity = rb.linearVelocity * 1.05f;
            }
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

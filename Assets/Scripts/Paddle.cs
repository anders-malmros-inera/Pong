using UnityEngine;
// Input System support (fallbacks to legacy Input if not available at runtime)
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Paddle : MonoBehaviour
{
    public float speed = 8f;
    public bool isLeft = true; // left or right paddle
    public bool useAI = false; // simple AI following the ball

    Rigidbody2D rb;
    Transform ballTransform;
    BoxCollider2D paddleCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        paddleCollider = GetComponent<BoxCollider2D>();
        if (paddleCollider == null)
        {
            // Ensure we have a collider so clamping calculations work
            paddleCollider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    void Start()
    {
        var ball = FindAnyObjectByType<Ball>();
        if (ball != null) ballTransform = ball.transform;
    }

    void FixedUpdate()
    {
        float move = 0f;

        if (useAI && ballTransform != null)
        {
            float dir = ballTransform.position.y - transform.position.y;
            move = Mathf.Sign(dir) * speed;

            // dampen small movements
            if (Mathf.Abs(dir) < 0.2f) move = 0f;
        }
        else
        {
            if (isLeft)
            {
                // W / S
                float up = IsPressed(KeyCode.W) ? 1f : 0f;
                float down = IsPressed(KeyCode.S) ? -1f : 0f;
                move = (up + down) * speed;
            }
            else
            {
                // P / L (right paddle)
                float up = IsPressed(KeyCode.P) ? 1f : 0f;
                float down = IsPressed(KeyCode.L) ? -1f : 0f;
                move = (up + down) * speed;
            }
        }

        // Move using MovePosition for kinematic Rigidbody2D and clamp using collider bounds
        float moveAmount = 0f;
        if (useAI && ballTransform != null)
        {
            float dir = ballTransform.position.y - transform.position.y;
            // Move towards the ball but cap movement per FixedUpdate
            moveAmount = Mathf.Clamp(dir, -speed * Time.fixedDeltaTime, speed * Time.fixedDeltaTime);
            if (Mathf.Abs(dir) < 0.02f) moveAmount = 0f;
        }
        else
        {
            // Player input produces -1, 0 or 1 then scaled by speed and dt
            float input = 0f;
            if (isLeft)
            {
                input = (IsPressed(KeyCode.W) ? 1f : 0f) + (IsPressed(KeyCode.S) ? -1f : 0f);
            }
            else
            {
                input = (IsPressed(KeyCode.P) ? 1f : 0f) + (IsPressed(KeyCode.L) ? -1f : 0f);
            }
            moveAmount = input * speed * Time.fixedDeltaTime;
        }

        float targetY = transform.position.y + moveAmount;

        // clamp using collider bounds (more accurate than localScale)
        float cameraHeight = Camera.main != null ? Camera.main.orthographicSize : 5f;
        float halfHeight = (paddleCollider != null) ? paddleCollider.bounds.extents.y : (transform.localScale.y / 2f);
        float minY = -cameraHeight + halfHeight;
        float maxY = cameraHeight - halfHeight;
        targetY = Mathf.Clamp(targetY, minY, maxY);

        rb.MovePosition(new Vector2(transform.position.x, targetY));
    }

    // Cross-compatible key check: prefer Input System if available, otherwise use legacy Input
    bool IsPressed(KeyCode key)
    {
        // If Input System package is present and a keyboard is available, use it.
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            switch (key)
            {
                case KeyCode.W: return keyboard.wKey.isPressed;
                case KeyCode.S: return keyboard.sKey.isPressed;
                case KeyCode.P: return keyboard.pKey.isPressed;
                case KeyCode.L: return keyboard.lKey.isPressed;
                case KeyCode.UpArrow: return keyboard.upArrowKey.isPressed;
                case KeyCode.DownArrow: return keyboard.downArrowKey.isPressed;
                default: return false;
            }
        }

        // Fallback to legacy Input (works when old system active)
        try
        {
            return Input.GetKey(key);
        }
        catch (System.InvalidOperationException)
        {
            // If legacy Input is not allowed (Input System active), just return false here.
            return false;
        }
    }
}

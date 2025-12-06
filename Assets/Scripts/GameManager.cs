using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int scoreLeft = 0;
    public int scoreRight = 0;
    public int winningScore = 10;
    public Ball ball;
    public UIManager ui;

    bool isGameOver = false;
    bool leftAccepted = false;
    bool rightAccepted = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (ui != null) ui.UpdateScore(scoreLeft, scoreRight);
    }

    void Update()
    {
        if (!isGameOver) return;

        // Check acceptance keys: left = W, right = P
        if (!leftAccepted && IsKeyDown(KeyCode.W))
        {
            leftAccepted = true;
            if (ui != null) ui.SetPlayAccepted(0, true);
        }
        if (!rightAccepted && IsKeyDown(KeyCode.P))
        {
            rightAccepted = true;
            if (ui != null) ui.SetPlayAccepted(1, true);
        }
    }

    bool IsKeyDown(KeyCode key)
    {
        // Prefer new Input System if present
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null)
        {
            switch (key)
            {
                case KeyCode.W: return keyboard.wKey.wasPressedThisFrame;
                case KeyCode.P: return keyboard.pKey.wasPressedThisFrame;
                default: return false;
            }
        }

        try
        {
            return Input.GetKeyDown(key);
        }
        catch (System.InvalidOperationException)
        {
            return false;
        }
    }

    public void ScorePoint(int playerIndex)
    {
        if (isGameOver) return; // ignore scoring while waiting for restart

        if (playerIndex == 0) scoreLeft++;
        else scoreRight++;

        if (ui != null) ui.UpdateScore(scoreLeft, scoreRight);

        // If a player reached winningScore, handle win
        if (scoreLeft >= winningScore)
        {
            StartCoroutine(HandleWin(0));
        }
        else if (scoreRight >= winningScore)
        {
            StartCoroutine(HandleWin(1));
        }
        else
        {
            StartCoroutine(DoReset());
        }
    }

    IEnumerator HandleWin(int playerIndex)
    {
        isGameOver = true;
        leftAccepted = false;
        rightAccepted = false;

        if (ball != null) ball.gameObject.SetActive(false);

        if (ui != null)
        {
            ui.ShowWinner(playerIndex);
            ui.ShowPlayAgainPrompts();
            ui.SetPlayAccepted(0, false);
            ui.SetPlayAccepted(1, false);
        }

        // Wait until both players accepted
        while (!(leftAccepted && rightAccepted))
        {
            yield return null;
        }

        // Both accepted: start 5s countdown
        if (ui != null) yield return ui.ShowCountdown(5f);
        else yield return new WaitForSeconds(5f);

        // Reset for new game
        scoreLeft = 0;
        scoreRight = 0;
        if (ui != null) ui.UpdateScore(scoreLeft, scoreRight);
        if (ui != null) ui.HideAllPrompts();
        isGameOver = false;

        if (ball != null)
        {
            ball.gameObject.SetActive(true);
            ball.ResetBall(Random.value > 0.5f);
        }
    }

    IEnumerator DoReset()
    {
        if (ball != null)
        {
            ball.gameObject.SetActive(false);
        }

        // Show a 5 second countdown in the center of the screen when available
        if (ui != null)
        {
            // yield return the UIManager coroutine so we wait for it to finish
            yield return ui.ShowCountdown(5f);
        }
        else
        {
            // fallback wait
            yield return new WaitForSeconds(5f);
        }

        if (ball != null)
        {
            ball.gameObject.SetActive(true);
            ball.ResetBall(Random.value > 0.5f);
        }
    }
}

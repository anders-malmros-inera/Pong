using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int scoreLeft = 0;
    public int scoreRight = 0;
    public Ball ball;
    public UIManager ui;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (ui != null) ui.UpdateScore(scoreLeft, scoreRight);
    }

    public void ScorePoint(int playerIndex)
    {
        if (playerIndex == 0) scoreLeft++;
        else scoreRight++;

        if (ui != null) ui.UpdateScore(scoreLeft, scoreRight);

        StartCoroutine(DoReset());
    }

    IEnumerator DoReset()
    {
        if (ball != null)
        {
            ball.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(1f);

        if (ball != null)
        {
            ball.gameObject.SetActive(true);
            ball.ResetBall(Random.value > 0.5f);
        }
    }
}

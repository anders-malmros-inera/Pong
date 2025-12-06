using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text leftScoreText;
    public Text rightScoreText;
    public Text countdownText;
    public Text winnerLeftText;
    public Text winnerRightText;
    public Text playAgainLeftText;
    public Text playAgainRightText;

    public void UpdateScore(int left, int right)
    {
        if (leftScoreText != null) leftScoreText.text = left.ToString();
        if (rightScoreText != null) rightScoreText.text = right.ToString();
    }

    public System.Collections.IEnumerator ShowCountdown(float seconds)
    {
        if (countdownText == null)
            yield break;

        countdownText.gameObject.SetActive(true);

        int remaining = Mathf.CeilToInt(seconds);
        while (remaining > 0)
        {
            countdownText.text = remaining.ToString();
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        // hide when reaches 0
        countdownText.gameObject.SetActive(false);
    }

    public void ShowWinner(int playerIndex)
    {
        HideAllPrompts();
        if (playerIndex == 0 && winnerLeftText != null)
        {
            winnerLeftText.gameObject.SetActive(true);
            winnerLeftText.text = "WINNER!";
        }
        else if (playerIndex == 1 && winnerRightText != null)
        {
            winnerRightText.gameObject.SetActive(true);
            winnerRightText.text = "WINNER!";
        }
    }

    public void ShowPlayAgainPrompts()
    {
        if (playAgainLeftText != null)
        {
            playAgainLeftText.gameObject.SetActive(true);
            playAgainLeftText.text = "Play Again? (W)";
            playAgainLeftText.color = Color.white;
        }
        if (playAgainRightText != null)
        {
            playAgainRightText.gameObject.SetActive(true);
            playAgainRightText.text = "Play Again? (P)";
            playAgainRightText.color = Color.white;
        }
    }

    public void SetPlayAccepted(int playerIndex, bool accepted)
    {
        if (playerIndex == 0 && playAgainLeftText != null)
        {
            playAgainLeftText.text = accepted ? "Accepted" : "Play Again? (W)";
            playAgainLeftText.color = accepted ? Color.green : Color.white;
        }
        else if (playerIndex == 1 && playAgainRightText != null)
        {
            playAgainRightText.text = accepted ? "Accepted" : "Play Again? (P)";
            playAgainRightText.color = accepted ? Color.green : Color.white;
        }
    }

    public void HideAllPrompts()
    {
        if (winnerLeftText != null) winnerLeftText.gameObject.SetActive(false);
        if (winnerRightText != null) winnerRightText.gameObject.SetActive(false);
        if (playAgainLeftText != null) playAgainLeftText.gameObject.SetActive(false);
        if (playAgainRightText != null) playAgainRightText.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }
}

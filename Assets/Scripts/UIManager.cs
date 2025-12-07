using System.Collections;
using System.Reflection;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Use GameObjects for UI so we don't require the uGUI assembly to compile.
    public GameObject leftScoreObj;
    public GameObject rightScoreObj;
    public GameObject countdownObj;
    public GameObject winnerLeftObj;
    public GameObject winnerRightObj;
    public GameObject playAgainLeftObj;
    public GameObject playAgainRightObj;

    void SetTextOn(GameObject go, string text)
    {
        if (go == null) return;
        var comp = go.GetComponent("Text");
        if (comp == null) return;
        var prop = comp.GetType().GetProperty("text");
        if (prop != null) prop.SetValue(comp, text, null);
    }

    void SetColorOn(GameObject go, Color c)
    {
        if (go == null) return;
        var comp = go.GetComponent("Text");
        if (comp == null) return;
        var prop = comp.GetType().GetProperty("color");
        if (prop != null) prop.SetValue(comp, c, null);
    }

    void SetActiveObj(GameObject go, bool active)
    {
        if (go == null) return;
        go.SetActive(active);
    }

    public void UpdateScore(int left, int right)
    {
        SetTextOn(leftScoreObj, left.ToString());
        SetTextOn(rightScoreObj, right.ToString());
    }

    public IEnumerator ShowCountdown(float seconds)
    {
        if (countdownObj == null) yield break;
        SetActiveObj(countdownObj, true);
        int remaining = Mathf.CeilToInt(seconds);
        while (remaining > 0)
        {
            SetTextOn(countdownObj, remaining.ToString());
            yield return new WaitForSeconds(1f);
            remaining--;
        }
        SetActiveObj(countdownObj, false);
    }

    public void ShowWinner(int playerIndex)
    {
        HideAllPrompts();
        if (playerIndex == 0)
        {
            SetActiveObj(winnerLeftObj, true);
            SetTextOn(winnerLeftObj, "WINNER!");
        }
        else
        {
            SetActiveObj(winnerRightObj, true);
            SetTextOn(winnerRightObj, "WINNER!");
        }
    }

    public void ShowPlayAgainPrompts()
    {
        SetActiveObj(playAgainLeftObj, true);
        SetTextOn(playAgainLeftObj, "Play Again? (W)");
        SetColorOn(playAgainLeftObj, Color.white);

        SetActiveObj(playAgainRightObj, true);
        SetTextOn(playAgainRightObj, "Play Again? (P)");
        SetColorOn(playAgainRightObj, Color.white);
    }

    public void SetPlayAccepted(int playerIndex, bool accepted)
    {
        if (playerIndex == 0)
        {
            SetTextOn(playAgainLeftObj, accepted ? "Accepted" : "Play Again? (W)");
            SetColorOn(playAgainLeftObj, accepted ? Color.green : Color.white);
        }
        else
        {
            SetTextOn(playAgainRightObj, accepted ? "Accepted" : "Play Again? (P)");
            SetColorOn(playAgainRightObj, accepted ? Color.green : Color.white);
        }
    }

    public void HideAllPrompts()
    {
        SetActiveObj(winnerLeftObj, false);
        SetActiveObj(winnerRightObj, false);
        SetActiveObj(playAgainLeftObj, false);
        SetActiveObj(playAgainRightObj, false);
        SetActiveObj(countdownObj, false);
    }
}

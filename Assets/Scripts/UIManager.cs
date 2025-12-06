using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text leftScoreText;
    public Text rightScoreText;

    public void UpdateScore(int left, int right)
    {
        if (leftScoreText != null) leftScoreText.text = left.ToString();
        if (rightScoreText != null) rightScoreText.text = right.ToString();
    }
}

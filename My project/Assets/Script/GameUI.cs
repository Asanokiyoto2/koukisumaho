using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text comboText;
    public Text scoreText;

    public void UpdateUI(
        int combo,
        int score)
    {
        if (comboText != null)
        {
            comboText.text =
                combo + " Combo!";
        }

        if (scoreText != null)
        {
            scoreText.text =
                "Score : " + score;
        }
    }
}
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("ÉXÉRÉA")]
    public int scorePerDrop = 100;

    public int Combo { get; private set; }

    public int Score { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResetCombo();
    }

    public void ResetCombo()
    {
        Combo = 0;
    }

    public void AddCombo(
        int destroyedCount)
    {
        Combo++;

        int addScore =
            destroyedCount *
            scorePerDrop *
            Combo;

        Score += addScore;

        Debug.Log(
            Combo +
            " Combo / " +
            destroyedCount +
            "å¬è¡ãé / Score = " +
            Score
        );

        GameUI ui =
            FindFirstObjectByType<GameUI>();

        if (ui != null)
        {
            ui.UpdateUI(
                Combo,
                Score
            );
        }
    }
}

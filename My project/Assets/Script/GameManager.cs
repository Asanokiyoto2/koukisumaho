
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("スコア設定")]
    public int scorePerDrop = 100;

    public int Combo { get; private set; }
    public int Score { get; private set; }

    public bool CanPlay { get; private set; } = true;

    private GameUI gameUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        gameUI = FindFirstObjectByType<GameUI>();

        Combo = 0;
        Score = 0;
        CanPlay = true;

        UpdateUI();
    }

    public void BeginMove()
    {
        Combo = 0;
        UpdateUI();
    }

    public void AddCombo(
        int destroyedCount,
        int comboNumber,
        int[] typeCounts)
    {
        if (!CanPlay)
            return;

        Combo = comboNumber;

        int multiplier = Mathf.Max(1, comboNumber);
        int addedScore =
            destroyedCount * scorePerDrop * multiplier;

        Score += addedScore;

        string message =
            comboNumber + " Combo! " +
            destroyedCount + "個消去！ +" +
            addedScore + "点";

        Debug.Log(message);

        UpdateUI();
    }

    public void FinishMove(int combo, int totalCleared)
    {
        if (combo == 0)
        {
            Combo = 0;
            Debug.Log("消去なし");
        }
        else
        {
            Debug.Log(
                "合計 " + combo +
                "コンボ / " + totalCleared +
                "個消去 / スコア " + Score
            );
        }

        UpdateUI();
    }

    public void ResetGame()
    {
        Combo = 0;
        Score = 0;
        CanPlay = true;

        BoardManager board =
            FindFirstObjectByType<BoardManager>();

        if (board != null)
            board.ResetBoard();

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (gameUI == null)
            gameUI = FindFirstObjectByType<GameUI>();

        if (gameUI != null)
            gameUI.UpdateUI(Combo, Score);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}


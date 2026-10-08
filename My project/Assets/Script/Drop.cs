using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Drop : MonoBehaviour
{
    public DropType Type { get; private set; }

    public int X { get; private set; }
    public int Y { get; private set; }

    private BoardManager board;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(
        BoardManager board,
        int x,
        int y,
        DropType type)
    {
        this.board = board;

        SetGridPosition(x, y);
        SetType(type);
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;

        if (board != null)
        {
            transform.localPosition =
                board.GridToLocalPosition(x, y);
        }
    }

    public void SetType(DropType type)
    {
        Type = type;

        switch (type)
        {
            case DropType.Red:
                spriteRenderer.color = Color.red;
                break;

            case DropType.Blue:
                spriteRenderer.color = Color.blue;
                break;

            case DropType.Green:
                spriteRenderer.color = Color.green;
                break;

            case DropType.Yellow:
                spriteRenderer.color = Color.yellow;
                break;

            case DropType.Purple:
                spriteRenderer.color =
                    new Color(0.65f, 0.2f, 0.8f);
                break;
        }
    }
}

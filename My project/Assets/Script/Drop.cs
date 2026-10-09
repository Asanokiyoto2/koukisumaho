
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
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Initialize(
        BoardManager boardManager,
        int x,
        int y,
        DropType type)
    {
        board = boardManager;
        X = x;
        Y = y;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        SetType(type);
        SetGridPosition(x, y);
    }

    public void SetType(DropType type)
    {
        Type = type;

        if (board == null)
            return;

        Sprite sprite = board.GetSpriteForType(type);

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = board.GetColorForType(type);

        // 画像の縦横比を保ちながら、1マスに収まる大きさにする
        if (sprite != null)
        {
            float maxDimension = Mathf.Max(
                sprite.bounds.size.x,
                sprite.bounds.size.y
            );

            if (maxDimension > 0f)
            {
                float scale = board.cellSize * 0.9f / maxDimension;
                transform.localScale = new Vector3(scale, scale, 1f);

                // 当たり判定もマスに収まるようにする
                boxCollider.size = new Vector2(
                    board.cellSize * 0.85f / scale,
                    board.cellSize * 0.85f / scale
                );
            }
        }
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;

        if (board != null)
            transform.localPosition = board.GridToLocalPosition(x, y);
    }
}



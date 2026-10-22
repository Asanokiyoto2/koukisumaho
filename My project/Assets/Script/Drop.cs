
using System.Collections;
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
    private SpriteRenderer highlightRenderer;
    private BoxCollider2D boxCollider;
    private Coroutine moveCoroutine;

    private int originalSortingOrder;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        originalSortingOrder = spriteRenderer.sortingOrder;
        CreateHighlight();
    }

    private void CreateHighlight()
    {
        GameObject highlight = new GameObject("SelectionHighlight");
        highlight.transform.SetParent(transform, false);
        highlight.transform.localPosition = Vector3.zero;
        highlight.transform.localScale = Vector3.one * 1.2f;

        highlightRenderer = highlight.AddComponent<SpriteRenderer>();
        highlightRenderer.sprite = spriteRenderer.sprite;
        highlightRenderer.color = new Color(1f, 0.65f, 0.05f, 0.9f);
        highlightRenderer.sortingOrder = originalSortingOrder - 1;
        highlightRenderer.enabled = false;
    }

    public void Initialize(
        BoardManager boardManager, int x, int y, DropType type)
    {
        board = boardManager;

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

        if (highlightRenderer != null)
            highlightRenderer.sprite = sprite;

        if (sprite != null)
        {
            float maxDimension = Mathf.Max(
                sprite.bounds.size.x,
                sprite.bounds.size.y);

            if (maxDimension > 0f)
            {
                float scale = board.cellSize * 0.9f / maxDimension;

                transform.localScale = new Vector3(scale, scale, 1f);

                boxCollider.size = new Vector2(
                    board.cellSize * 0.85f / scale,
                    board.cellSize * 0.85f / scale);
            }
        }
    }

    // 選択中のドロップを目立たせる
    public void SetSelected(bool selected)
    {
        if (highlightRenderer != null)
            highlightRenderer.enabled = selected;

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = selected
                ? originalSortingOrder + 10
                : originalSortingOrder;
        }
    }

    // 位置を即座に変更する
    public void SetGridPosition(int x, int y)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        X = x;
        Y = y;

        if (board != null)
            transform.localPosition = board.GridToLocalPosition(x, y);
    }

    // 位置を滑らかに変更する
    public void SetGridPositionSmooth(int x, int y)
    {
        X = x;
        Y = y;

        if (board == null)
            return;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        Vector3 target = board.GridToLocalPosition(x, y);
        moveCoroutine = StartCoroutine(AnimatePosition(target));
    }

    private IEnumerator AnimatePosition(Vector3 target)
    {
        Vector3 start = transform.localPosition;
        float duration = 0.09f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 最初と最後が自然につながる動き
            t = t * t * (3f - 2f * t);

            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localPosition = target;
        moveCoroutine = null;
    }
}





using UnityEngine;
using UnityEngine.InputSystem;

public class DropInput : MonoBehaviour
{
    public BoardManager board;

    private Camera mainCamera;
    private Drop selectedDrop;
    private bool moved;

    // マウスを押した位置と石の位置のずれ
    private Vector3 dragOffset;

    // 最後に入れ替えたマス
    private int lastGridX;
    private int lastGridY;

    private void Start()
    {
        mainCamera = Camera.main;

        if (board == null)
            board = FindFirstObjectByType<BoardManager>();

        if (mainCamera == null)
            Debug.LogError("Main Cameraが見つかりません。");

        if (board == null)
            Debug.LogError("BoardManagerが見つかりません。");
    }

    private void Update()
    {
        if (board == null || mainCamera == null ||
            Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!board.IsBusy &&
                (GameManager.Instance == null ||
                 GameManager.Instance.CanPlay))
            {
                BeginDrag();
            }
        }

        if (selectedDrop != null &&
            Mouse.current.leftButton.isPressed &&
            !board.IsBusy)
        {
            ContinueDrag();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            EndDrag();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 screen = Mouse.current.position.ReadValue();

        float distance = -mainCamera.transform.position.z;

        Vector3 world = mainCamera.ScreenToWorldPoint(
            new Vector3(screen.x, screen.y, distance));

        world.z = 0f;
        return world;
    }

    private Vector2 GetMouseBoardPosition()
    {
        Vector3 local = board.transform.InverseTransformPoint(
            GetMouseWorldPosition());

        return new Vector2(local.x, local.y);
    }

    private Vector2Int GetGridPosition(Vector2 local)
    {
        int x = Mathf.RoundToInt(
            local.x / board.cellSize +
            (board.width - 1) * 0.5f);

        int y = Mathf.RoundToInt(
            local.y / board.cellSize +
            (board.height - 1) * 0.5f);

        return new Vector2Int(x, y);
    }

    private void BeginDrag()
    {
        Collider2D hit = Physics2D.OverlapPoint(
            GetMouseWorldPosition());

        if (hit == null)
            return;

        selectedDrop = hit.GetComponent<Drop>();

        if (selectedDrop == null)
            return;

        moved = false;

        Vector3 mouseLocal = board.transform.InverseTransformPoint(
            GetMouseWorldPosition());

        dragOffset = selectedDrop.transform.localPosition -
                     mouseLocal;

        lastGridX = selectedDrop.X;
        lastGridY = selectedDrop.Y;

        selectedDrop.SetSelected(true);
        board.BeginDragMove();
    }

    private void ContinueDrag()
    {
        if (selectedDrop == null || board.IsBusy)
            return;

        Vector2 mouseLocal = GetMouseBoardPosition();

        // 石の見た目をマウスに直接追従させる
        Vector3 visualPosition = new Vector3(
            mouseLocal.x + dragOffset.x,
            mouseLocal.y + dragOffset.y,
            selectedDrop.transform.localPosition.z);

        selectedDrop.transform.localPosition = visualPosition;

        Vector2Int target = GetGridPosition(mouseLocal);

        if (target.x < 0 || target.x >= board.width ||
            target.y < 0 || target.y >= board.height)
            return;

        // 同じマスなら何もしない
        if (target.x == lastGridX && target.y == lastGridY)
            return;

        // 1フレームで複数マス移動しても、
        // 通過したマスを順番に処理する
        int safety = board.width + board.height;

        while ((lastGridX != target.x ||
                lastGridY != target.y) &&
               safety-- > 0)
        {
            int nextX = lastGridX;
            int nextY = lastGridY;

            if (nextX != target.x)
                nextX += nextX < target.x ? 1 : -1;
            else
                nextY += nextY < target.y ? 1 : -1;

            if (!board.MoveDraggedDropTo(
                selectedDrop, nextX, nextY))
                break;

            lastGridX = nextX;
            lastGridY = nextY;
            moved = true;
        }
    }

    private void EndDrag()
    {
        if (selectedDrop == null)
            return;

        selectedDrop.SetSelected(false);

        // 最後に確定した盤面位置に合わせる
        selectedDrop.transform.localPosition =
            board.GridToLocalPosition(
                selectedDrop.X,
                selectedDrop.Y);

        if (moved)
            board.FinishMove();
        else
            board.CancelDragMove();

        selectedDrop = null;
        moved = false;
    }

    private void OnDisable()
    {
        if (selectedDrop != null)
        {
            selectedDrop.SetSelected(false);
            selectedDrop.SetGridPosition(
                selectedDrop.X,
                selectedDrop.Y);
        }

        if (board != null)
            board.CancelDragMove();

        selectedDrop = null;
        moved = false;
    }
}





using UnityEngine;
using UnityEngine.InputSystem;

public class DropInput : MonoBehaviour
{
    public BoardManager board;

    private Camera mainCamera;
    private Drop selectedDrop;
    private bool moved;

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
        if (board == null || mainCamera == null)
            return;

        if (Mouse.current == null)
            return;

        // ドラッグ中は、操作を最後まで受け付ける
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
        {
            EndDrag();
        }
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

        selectedDrop.SetSelected(true);
        board.BeginDragMove();
    }

    private void ContinueDrag()
    {
        if (selectedDrop == null || board.IsBusy)
            return;

        Vector3 localPosition =
            board.transform.InverseTransformPoint(
                GetMouseWorldPosition());

        int targetX = Mathf.RoundToInt(
            (localPosition.x +
             (board.width - 1) * board.cellSize / 2f)
            / board.cellSize);

        int targetY = Mathf.RoundToInt(
            (localPosition.y +
             (board.height - 1) * board.cellSize / 2f)
            / board.cellSize);

        if (targetX < 0 || targetX >= board.width ||
            targetY < 0 || targetY >= board.height)
            return;

        if (board.MoveDraggedDropTo(
            selectedDrop, targetX, targetY))
        {
            moved = true;
        }

        // 重要：
        // selectedDrop.transform.localPosition を
        // マウス位置へ直接変更しない。
        // BoardManager側の滑らかな移動に任せる。
    }

    private void EndDrag()
    {
        if (selectedDrop == null)
            return;

        selectedDrop.SetSelected(false);

        // 現在のマス位置に合わせる
        selectedDrop.SetGridPosition(
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



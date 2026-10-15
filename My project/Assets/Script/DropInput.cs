
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
    }

    private void Update()
    {
        if (board == null || mainCamera == null)
            return;

        if (Mouse.current == null)
            return;

        if (board.IsBusy)
            return;

        if (GameManager.Instance != null &&
            !GameManager.Instance.CanPlay)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            BeginDrag();

        if (Mouse.current.leftButton.isPressed)
            ContinueDrag();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            EndDrag();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 screen =
            Mouse.current.position.ReadValue();

        float distance =
            -mainCamera.transform.position.z;

        Vector3 world = mainCamera.ScreenToWorldPoint(
            new Vector3(screen.x, screen.y, distance)
        );

        world.z = 0f;
        return world;
    }

    private void BeginDrag()
    {
        Collider2D hit =
            Physics2D.OverlapPoint(GetMouseWorldPosition());

        if (hit == null)
            return;

        selectedDrop = hit.GetComponent<Drop>();

        if (selectedDrop == null)
            return;

        moved = false;

        // ドラッグ開始前の盤面を保存
        board.BeginDragMove();
    }

    private void ContinueDrag()
    {
        if (selectedDrop == null || board.IsBusy)
            return;

        Vector3 localPosition =
            board.transform.InverseTransformPoint(
                GetMouseWorldPosition()
            );

        int targetX = Mathf.RoundToInt(
            (localPosition.x +
             (board.width - 1) * board.cellSize / 2f)
            / board.cellSize
        );

        int targetY = Mathf.RoundToInt(
            (localPosition.y +
             (board.height - 1) * board.cellSize / 2f)
            / board.cellSize
        );

        if (targetX < 0 || targetX >= board.width ||
            targetY < 0 || targetY >= board.height)
            return;

        if (board.MoveDraggedDropTo(selectedDrop,targetX,targetY))
        {
            moved = true;
        }
    }

    private void EndDrag()
    {
        if (selectedDrop != null && moved)
        {
            // 指を離したタイミングで消去判定
            board.FinishMove();
        }
        else
        {
            // 移動していなければ保存を解除
            board.CancelDragMove();
        }

        selectedDrop = null;
        moved = false;
    }

    private void OnDisable()
    {
        if (board != null)
            board.CancelDragMove();

        selectedDrop = null;
        moved = false;
    }
}


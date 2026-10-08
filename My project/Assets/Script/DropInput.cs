using UnityEngine;

public class DropInput : MonoBehaviour
{
    public BoardManager board;

    private Drop selectedDrop;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (board == null)
            return;

        if (board.IsBusy)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }

        if (Input.GetMouseButton(0))
        {
            Drag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }

    // =====================================================
    // íÕÇﬁ
    // =====================================================

    private void StartDrag()
    {
        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

        worldPosition.z = 0f;

        Collider2D hit =
            Physics2D.OverlapPoint(
                worldPosition
            );

        if (hit == null)
            return;

        selectedDrop =
            hit.GetComponent<Drop>();
    }

    // =====================================================
    // à⁄ìÆ
    // =====================================================

    private void Drag()
    {
        if (selectedDrop == null)
            return;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                Input.mousePosition
            );

        worldPosition.z = 0f;

        Vector3 localPosition =
            board.transform
            .InverseTransformPoint(
                worldPosition
            );

        int targetX =
            Mathf.RoundToInt(
                (
                    localPosition.x +
                    (board.width - 1) *
                    board.cellSize /
                    2f
                )
                /
                board.cellSize
            );

        int targetY =
            Mathf.RoundToInt(
                (
                    localPosition.y +
                    (board.height - 1) *
                    board.cellSize /
                    2f
                )
                /
                board.cellSize
            );

        targetX =
            Mathf.Clamp(
                targetX,
                0,
                board.width - 1
            );

        targetY =
            Mathf.Clamp(
                targetY,
                0,
                board.height - 1
            );

        Drop target =
            board.GetDrop(
                targetX,
                targetY
            );

        if (target == null)
            return;

        if (target == selectedDrop)
            return;

        int dx =
            Mathf.Abs(
                target.X -
                selectedDrop.X
            );

        int dy =
            Mathf.Abs(
                target.Y -
                selectedDrop.Y
            );

        // ó◊ê⁄ÇµÇƒÇ¢ÇÈèÍçáÇæÇØåä∑
        if (dx + dy == 1)
        {
            board.Swap(
                selectedDrop,
                target
            );
        }
    }

    // =====================================================
    // ó£Ç∑
    // =====================================================

    private void EndDrag()
    {
        selectedDrop = null;
    }
}

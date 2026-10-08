using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("盤面サイズ")]
    public int width = 6;
    public int height = 6;

    [Header("ドロップ")]
    public Drop dropPrefab;

    [Header("マスの間隔")]
    public float cellSize = 1.1f;

    [Header("消去演出")]
    public float destroyDelay = 0.15f;

    [Header("落下演出")]
    public float fallDelay = 0.15f;

    private Drop[,] board;

    public bool IsBusy { get; private set; }

    private void Start()
    {
        board = new Drop[width, height];

        CreateInitialBoard();
    }

    // =====================================================
    // 初期盤面
    // =====================================================

    private void CreateInitialBoard()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateSafeDrop(x, y);
            }
        }
    }

    private void CreateSafeDrop(int x, int y)
    {
        List<DropType> types =
            new List<DropType>();

        for (int i = 0; i < 5; i++)
        {
            types.Add((DropType)i);
        }

        while (types.Count > 0)
        {
            int index =
                Random.Range(0, types.Count);

            DropType type =
                types[index];

            bool horizontal =
                CheckHorizontalStart(
                    x,
                    y,
                    type
                );

            bool vertical =
                CheckVerticalStart(
                    x,
                    y,
                    type
                );

            if (!horizontal && !vertical)
            {
                CreateDrop(
                    x,
                    y,
                    type
                );

                return;
            }

            types.RemoveAt(index);
        }

        CreateDrop(
            x,
            y,
            GetRandomDropType()
        );
    }

    private bool CheckHorizontalStart(
        int x,
        int y,
        DropType type)
    {
        if (x < 2)
            return false;

        if (board[x - 1, y] == null ||
            board[x - 2, y] == null)
        {
            return false;
        }

        return
            board[x - 1, y].Type == type &&
            board[x - 2, y].Type == type;
    }

    private bool CheckVerticalStart(
        int x,
        int y,
        DropType type)
    {
        if (y < 2)
            return false;

        if (board[x, y - 1] == null ||
            board[x, y - 2] == null)
        {
            return false;
        }

        return
            board[x, y - 1].Type == type &&
            board[x, y - 2].Type == type;
    }

    // =====================================================
    // ドロップ生成
    // =====================================================

    private void CreateDrop(
        int x,
        int y,
        DropType type)
    {
        Drop drop =
            Instantiate(
                dropPrefab,
                transform
            );

        drop.Initialize(
            this,
            x,
            y,
            type
        );

        board[x, y] = drop;
    }

    private DropType GetRandomDropType()
    {
        return (DropType)
            Random.Range(0, 5);
    }

    // =====================================================
    // 座標変換
    // =====================================================

    public Vector3 GridToLocalPosition(
        int x,
        int y)
    {
        float startX =
            -(width - 1) *
            cellSize /
            2f;

        float startY =
            -(height - 1) *
            cellSize /
            2f;

        return new Vector3(
            startX + x * cellSize,
            startY + y * cellSize,
            0f
        );
    }

    // =====================================================
    // ドロップ取得
    // =====================================================

    public Drop GetDrop(
        int x,
        int y)
    {
        if (x < 0 ||
            x >= width ||
            y < 0 ||
            y >= height)
        {
            return null;
        }

        return board[x, y];
    }

    // =====================================================
    // ドロップ交換
    // =====================================================

    public void Swap(
        Drop a,
        Drop b)
    {
        if (IsBusy)
            return;

        StartCoroutine(
            SwapRoutine(a, b)
        );
    }

    private IEnumerator SwapRoutine(
        Drop a,
        Drop b)
    {
        IsBusy = true;

        int ax = a.X;
        int ay = a.Y;

        int bx = b.X;
        int by = b.Y;

        board[ax, ay] = b;
        board[bx, by] = a;

        a.SetGridPosition(
            bx,
            by
        );

        b.SetGridPosition(
            ax,
            ay
        );

        yield return new WaitForSeconds(
            0.1f
        );

        List<Drop> matches =
            FindMatches();

        if (matches.Count > 0)
        {
            GameManager.Instance.ResetCombo();

            yield return ResolveBoard();
        }

        IsBusy = false;
    }

    // =====================================================
    // 盤面処理
    // =====================================================

    private IEnumerator ResolveBoard()
    {
        while (true)
        {
            List<Drop> matches =
                FindMatches();

            if (matches.Count == 0)
                break;

            GameManager.Instance.AddCombo(
                matches.Count
            );

            foreach (Drop drop in matches)
            {
                if (drop == null)
                    continue;

                board[
                    drop.X,
                    drop.Y
                ] = null;

                Destroy(
                    drop.gameObject
                );
            }

            yield return new WaitForSeconds(
                destroyDelay
            );

            CollapseBoard();

            yield return new WaitForSeconds(
                fallDelay
            );

            SpawnMissingDrops();

            yield return new WaitForSeconds(
                fallDelay
            );
        }
    }

    // =====================================================
    // マッチ検索
    // =====================================================

    private List<Drop> FindMatches()
    {
        HashSet<Drop> matches =
            new HashSet<Drop>();

        // -------------------------
        // 横
        // -------------------------

        for (int y = 0; y < height; y++)
        {
            int x = 0;

            while (x < width)
            {
                Drop current =
                    board[x, y];

                if (current == null)
                {
                    x++;
                    continue;
                }

                int count = 1;

                while (
                    x + count < width &&
                    board[x + count, y] != null &&
                    board[x + count, y].Type ==
                    current.Type
                )
                {
                    count++;
                }

                if (count >= 3)
                {
                    for (int i = 0;
                         i < count;
                         i++)
                    {
                        matches.Add(
                            board[x + i, y]
                        );
                    }
                }

                x += count;
            }
        }

        // -------------------------
        // 縦
        // -------------------------

        for (int x = 0; x < width; x++)
        {
            int y = 0;

            while (y < height)
            {
                Drop current =
                    board[x, y];

                if (current == null)
                {
                    y++;
                    continue;
                }

                int count = 1;

                while (
                    y + count < height &&
                    board[x, y + count] != null &&
                    board[x, y + count].Type ==
                    current.Type
                )
                {
                    count++;
                }

                if (count >= 3)
                {
                    for (int i = 0;
                         i < count;
                         i++)
                    {
                        matches.Add(
                            board[x, y + i]
                        );
                    }
                }

                y += count;
            }
        }

        return new List<Drop>(matches);
    }

    // =====================================================
    // 落下
    // =====================================================

    private void CollapseBoard()
    {
        for (int x = 0; x < width; x++)
        {
            int writeY = 0;

            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                    continue;

                if (writeY != y)
                {
                    Drop drop =
                        board[x, y];

                    board[x, writeY] =
                        drop;

                    board[x, y] =
                        null;

                    drop.SetGridPosition(
                        x,
                        writeY
                    );
                }

                writeY++;
            }
        }
    }

    // =====================================================
    // 空いた場所に生成
    // =====================================================

    private void SpawnMissingDrops()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                {
                    CreateDrop(
                        x,
                        y,
                        GetRandomDropType()
                    );
                }
            }
        }
    }
}

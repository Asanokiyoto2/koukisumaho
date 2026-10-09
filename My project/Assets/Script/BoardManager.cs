
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("盤面設定")]
    public int width = 6;
    public int height = 6;
    public float cellSize = 1.1f;

    [Header("5属性のドロップ画像")]
    public Sprite redSprite;
    public Sprite blueSprite;
    public Sprite greenSprite;
    public Sprite yellowSprite;
    public Sprite purpleSprite;

    [Header("カメラ設定")]
    public Camera targetCamera;
    public bool autoFitCamera = true;
    public float cameraPadding = 0.35f;

    [Header("演出設定")]
    public float clearDelay = 0.15f;
    public float fallDelay = 0.15f;

    private Drop[,] drops;
    private Sprite fallbackSprite;
    private float previousAspect;

    public bool IsBusy { get; private set; }

    private readonly DropType[] allTypes =
    {
        DropType.Red,
        DropType.Blue,
        DropType.Green,
        DropType.Yellow,
        DropType.Purple
    };

    private void Awake()
    {
        width = Mathf.Max(3, width);
        height = Mathf.Max(3, height);

        drops = new Drop[width, height];

        // 画像未設定時の仮画像用。設定済みの画像には影響しない
        fallbackSprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Start()
    {
        ResetBoard();
        FitCameraToBoard();
    }

    private void LateUpdate()
    {
        if (!autoFitCamera || targetCamera == null)
            return;

        // Gameビューの縦横比が変更されたときに再調整する
        if (!Mathf.Approximately(previousAspect, targetCamera.aspect))
            FitCameraToBoard();
    }

    public void ResetBoard()
    {
        if (drops == null)
            drops = new Drop[width, height];

        // 既存ドロップを削除
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        drops = new Drop[width, height];
        IsBusy = false;

        // 初期盤面で最初から3個以上揃わないように生成
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                DropType type = CreateSafeDrop(x, y);
                CreateDrop(x, y, type);
            }
        }

        FitCameraToBoard();
    }

    private DropType CreateSafeDrop(int x, int y)
    {
        List<DropType> candidates = new List<DropType>(allTypes);

        // 左に同じ色が2個ある場合、その色を候補から除く
        if (x >= 2 &&
            drops[x - 1, y] != null &&
            drops[x - 2, y] != null &&
            drops[x - 1, y].Type == drops[x - 2, y].Type)
        {
            candidates.Remove(drops[x - 1, y].Type);
        }

        // 下に同じ色が2個ある場合も除く
        if (y >= 2 &&
            drops[x, y - 1] != null &&
            drops[x, y - 2] != null &&
            drops[x, y - 1].Type == drops[x, y - 2].Type)
        {
            candidates.Remove(drops[x, y - 1].Type);
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private Drop CreateDrop(int x, int y, DropType type)
    {
        GameObject obj = new GameObject("Drop_" + x + "_" + y);
        obj.transform.SetParent(transform, false);

        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<BoxCollider2D>();

        Drop drop = obj.AddComponent<Drop>();
        drop.Initialize(this, x, y, type);

        drops[x, y] = drop;
        return drop;
    }

    public Vector3 GridToLocalPosition(int x, int y)
    {
        return new Vector3(
            (x - (width - 1) * 0.5f) * cellSize,
            (y - (height - 1) * 0.5f) * cellSize,
            0f
        );
    }

    public Drop GetDropAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return null;

        return drops[x, y];
    }

    public Drop GetDrop(int x, int y)
    {
        return GetDropAt(x, y);
    }

    public Sprite GetSpriteForType(DropType type)
    {
        switch (type)
        {
            case DropType.Red: return redSprite != null ? redSprite : fallbackSprite;
            case DropType.Blue: return blueSprite != null ? blueSprite : fallbackSprite;
            case DropType.Green: return greenSprite != null ? greenSprite : fallbackSprite;
            case DropType.Yellow: return yellowSprite != null ? yellowSprite : fallbackSprite;
            case DropType.Purple: return purpleSprite != null ? purpleSprite : fallbackSprite;
            default: return fallbackSprite;
        }
    }

    public Color GetColorForType(DropType type)
    {
        // 自分の画像が設定されていれば色を重ねず、そのまま表示する
        if (HasSpriteForType(type))
            return Color.white;

        // 未設定時だけ色付きの仮画像を表示する
        switch (type)
        {
            case DropType.Red: return Color.red;
            case DropType.Blue: return Color.blue;
            case DropType.Green: return Color.green;
            case DropType.Yellow: return Color.yellow;
            case DropType.Purple: return new Color(0.65f, 0.2f, 1f);
            default: return Color.white;
        }
    }

    private bool HasSpriteForType(DropType type)
    {
        switch (type)
        {
            case DropType.Red: return redSprite != null;
            case DropType.Blue: return blueSprite != null;
            case DropType.Green: return greenSprite != null;
            case DropType.Yellow: return yellowSprite != null;
            case DropType.Purple: return purpleSprite != null;
            default: return false;
        }
    }

    public bool SwapImmediate(Drop a, Drop b)
    {
        if (a == null || b == null || IsBusy)
            return false;

        if (Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) != 1)
            return false;

        int ax = a.X;
        int ay = a.Y;
        int bx = b.X;
        int by = b.Y;

        drops[ax, ay] = b;
        drops[bx, by] = a;

        a.SetGridPosition(bx, by);
        b.SetGridPosition(ax, ay);

        return true;
    }

    public void FinishMove()
    {
        if (!IsBusy)
            StartCoroutine(ResolveBoard());
    }

    private IEnumerator ResolveBoard()
    {
        IsBusy = true;

        if (GameManager.Instance != null)
            GameManager.Instance.BeginMove();

        int comboCount = 0;
        int totalCleared = 0;

        while (true)
        {
            HashSet<Drop> matches = FindMatches();

            if (matches.Count == 0)
                break;

            comboCount++;

            int[] typeCounts = new int[5];

            foreach (Drop drop in matches)
            {
                if (drop == null)
                    continue;

                typeCounts[(int)drop.Type]++;

                drops[drop.X, drop.Y] = null;
                Destroy(drop.gameObject);
            }
            totalCleared += matches.Count;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCombo(
                    matches.Count,
                    comboCount,
                    typeCounts
                );
            }

            yield return new WaitForSeconds(clearDelay);

            CollapseColumns();

            yield return new WaitForSeconds(fallDelay);
        }

        IsBusy = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishMove(
                comboCount,
                totalCleared
            );
        }
    }

    private HashSet<Drop> FindMatches()
    {
        HashSet<Drop> matches = new HashSet<Drop>();

        // 横方向の一致を調べる
        for (int y = 0; y < height; y++)
        {
            int x = 0;

            while (x < width)
            {
                Drop first = drops[x, y];

                if (first == null)
                {
                    x++;
                    continue;
                }

                int end = x + 1;

                while (end < width &&
                       drops[end, y] != null &&
                       drops[end, y].Type == first.Type)
                {
                    end++;
                }

                if (end - x >= 3)
                {
                    for (int i = x; i < end; i++)
                        matches.Add(drops[i, y]);
                }

                x = end;
            }
        }

        // 縦方向の一致を調べる
        for (int x = 0; x < width; x++)
        {
            int y = 0;

            while (y < height)
            {
                Drop first = drops[x, y];

                if (first == null)
                {
                    y++;
                    continue;
                }

                int end = y + 1;

                while (end < height &&
                       drops[x, end] != null &&
                       drops[x, end].Type == first.Type)
                {
                    end++;
                }

                if (end - y >= 3)
                {
                    for (int i = y; i < end; i++)
                        matches.Add(drops[x, i]);
                }

                y = end;
            }
        }

        return matches;
    }

    private void CollapseColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int writeY = 0;

            // 残ったドロップを下に詰める
            for (int y = 0; y < height; y++)
            {
                Drop drop = drops[x, y];

                if (drop == null)
                    continue;

                if (writeY != y)
                {
                    drops[x, writeY] = drop;
                    drops[x, y] = null;
                    drop.SetGridPosition(x, writeY);
                }

                writeY++;
            }

            // 空いた上のマスを新しいドロップで補充
            for (int y = writeY; y < height; y++)
            {
                DropType type = allTypes[Random.Range(0, allTypes.Length)];
                CreateDrop(x, y, type);
            }
        }
    }

    public void FitCameraToBoard()
    {
        if (!autoFitCamera || targetCamera == null)
            return;

        targetCamera.orthographic = true;

        float aspect = targetCamera.aspect;
        if (aspect <= 0f)
            aspect = 9f / 16f;

        float boardWidth = width * cellSize *
                          Mathf.Abs(transform.lossyScale.x);
        float boardHeight = height * cellSize *
                           Mathf.Abs(transform.lossyScale.y);

        // 横幅・高さの両方が画面内に収まるサイズを使う
        float sizeForHeight = boardHeight * 0.5f;
        float sizeForWidth = boardWidth / (2f * aspect);

        float requiredSize = Mathf.Max(sizeForHeight, sizeForWidth);
        targetCamera.orthographicSize = requiredSize + cameraPadding;

        Vector3 center = transform.position;
        Vector3 cameraPosition = targetCamera.transform.position;

        targetCamera.transform.position = new Vector3(
            center.x,
            center.y,
            cameraPosition.z
        );

        previousAspect = aspect;
    }

    private void OnDestroy()
    {
        if (fallbackSprite != null)
            Destroy(fallbackSprite);
    }
}



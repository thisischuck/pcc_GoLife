using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class GameOfLife : MonoBehaviour
{
    public GameObject cellParent;
    public GameObject cellHolder;
    public Vector2 gridSize;
    public bool isDrawing;
    [Tooltip("1- Threads, 2- Tasks, Default-No Threads")]
    public int threads;
    [HideInInspector]
    [Range(0, 100)]
    public int mutationPercentage;

    public int threadCount = 10;

    private int gridWitdh;
    private int gridHeight;

    private GameObject[,] objectsArray;
    private int[,] cellArray;

    private int frameCount = 0;
    private float timePassed = 0;

    #region ComputeShader

    public ComputeShader computeShader;
    public RenderTexture result;

    public GameObject plane;

    int k;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        k = computeShader.FindKernel("CSMain");
        gridWitdh = (int)gridSize.x;
        gridHeight = (int)gridSize.y;
        CreateGame();

        result = new RenderTexture(gridWitdh, gridHeight, 1);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();
        computeShader.SetTexture(k, "Result", result);
        computeShader.SetFloat("height", gridHeight);
    }

    void CreateGame()
    {
        cellArray = new int[gridWitdh, gridHeight];
        objectsArray = new GameObject[gridWitdh, gridHeight];
        FillArray(cellArray, gridWitdh, gridHeight);
    }

    void Update()
    {
        switch (threads)
        {
            case 1:
                UpdateThread();
                break;
            case 2:
                UpdateTask();
                break;
            default:
                UpdateNoThread();
                break;
        }

        if (isDrawing)
        {
            //DrawBoard();
            DrawTexture();
        }

        frameCount++;
        timePassed += Time.deltaTime;
        if (timePassed > 1)
        {
            Debug.Log("Framerate " + frameCount);
            frameCount = 0;
            timePassed = 0;
        }
    }

    void UpdateNoThread()
    {
        GameLoop();

        Debug.Log("Main Iteration");
    }

    void UpdateTask()
    {
        var listThreads = new List<Task>();
        int[,] temp = (int[,])cellArray.Clone();
        for (int x = 0; x < threadCount; x++)
            for (int y = 0; y < threadCount; y++)
            {
                Vector2Int a = new Vector2Int(x, y);
                var t = Task.Run(() =>
                {
                    GameLoopThreads(a, temp);
                });
                listThreads.Add(t);
            }

        foreach (var t in listThreads) t.Wait();

        Debug.Log("Task Iteration");
    }

    void UpdateThread()
    {
        var listThreads = new List<Thread>();
        int[,] temp = (int[,])cellArray.Clone();
        for (int x = 0; x < threadCount; x++)
            for (int y = 0; y < threadCount; y++)
            {
                Vector2Int a = new Vector2Int(x, y);
                var t = new Thread(() =>
                {
                    GameLoopThreads(a, temp);
                });
                t.Start();
                listThreads.Add(t);
            }

        foreach (var t in listThreads) t.Join();

        Debug.Log("Thread Iteration");
    }

    void DrawTexture()
    {
        var c = new ComputeBuffer(gridWitdh * gridHeight, sizeof(int));
        c.SetData(cellArray);

        computeShader.SetBuffer(k, "buffer", c);
        computeShader.Dispatch(k, gridWitdh / 8, gridHeight / 8, 1);

        plane.GetComponent<MeshRenderer>().material.mainTexture = result;
        c.Release();
    }

    void DrawBoard()
    {
        int nCells = gridHeight * gridWitdh;
        //draws the board
        for (int i = 0; i < gridWitdh; i++)
            for (int j = 0; j < gridHeight; j++)
            {
                if (objectsArray[i, j])
                {
                    var g = objectsArray[i, j];
                    g.GetComponent<SpriteRenderer>().color = cellArray[i, j] == 1 ? Color.red : Color.black;
                }
                else
                {
                    Vector3 pos = new Vector3(
                        i,
                        j,
                        0
                    );
                    var g = GameObject.Instantiate(cellParent, pos, transform.rotation, cellHolder.transform);
                    g.GetComponent<SpriteRenderer>().color = cellArray[i, j] == 1 ? Color.red : Color.black;
                    objectsArray[i, j] = g;
                }

            }
    }

    void GameLoopThreads(Vector2Int id, int[,] a)
    {
        Vector2Int start = new Vector2Int(
            id.x * gridWitdh / threadCount,
            id.y * gridHeight / threadCount
        );
        Vector2Int end = new Vector2Int(
            (id.x + 1) * gridWitdh / threadCount,
            (id.y + 1) * gridHeight / threadCount
        );
        Debug.Log(id + " " + start + " " + end);

        for (int i = start.x; i < end.x; i++)
            for (int j = start.y; j < end.y; j++)
            {
                cellArray[i, j] = DeadOrAlive(i, j, a);
            }
    }

    void GameLoop()
    {
        var temp = cellArray.Clone();

        for (int i = 0; i < gridWitdh; i++)
            for (int j = 0; j < gridHeight; j++)
            {
                cellArray[i, j] = DeadOrAlive(i, j, (int[,])temp);
            }
    }

    void FillArray(int[,] a, int w, int h)
    {
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                int per = Random.Range(0, 101);
                a[i, j] = per >= 50 ? 0 : 1;
            }
        }
    }

    int DeadOrAlive(int x, int y, int[,] a)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                if (Alive(i + x, j + y, a) == 1) count++;
            }

        if (count > 3 || count < 2)
            return 0;
        else if (count == 3) return 1;

        // Abs(- 1) = 1
        // Viva: Abs(1 - 1) = 0
        // Morta: Abs(0 - 1) = 1
        /*if (Random.Range(0, 100) < mutationPercentage)
            return Mathf.Abs(cellArray[x, y] - 1);*/
        return cellArray[x, y];
    }

    int Alive(int x, int y, int[,] a)
    {
        if (x > 0 && x < gridWitdh)
            if (y > 0 && y < gridHeight)
            {
                return a[x, y];
            }
        return -1;
    }
}

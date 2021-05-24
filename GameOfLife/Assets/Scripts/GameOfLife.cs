using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfLife : MonoBehaviour
{
    public GameObject cellParent;
    public Vector2 gridSize;
    [Range(0, 100)]
    public int mutationPercentage;

    private int gridWitdh;
    private int gridHeight;

    private GameObject[,] objectsArray;
    private int[,] cellArray;
    // Start is called before the first frame update
    void Start()
    {
        gridWitdh = (int)gridSize.x;
        gridHeight = (int)gridSize.y;
        CreateGame();

        this.GetComponent<Camera>().orthographicSize = gridWitdh;
    }

    void Update()
    {
        GameLoop();
        DrawBoard();
    }

    void CreateGame()
    {
        cellArray = new int[gridWitdh, gridHeight];
        objectsArray = new GameObject[gridWitdh, gridHeight];
        FillArray(cellArray, gridWitdh, gridHeight);
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
                    var g = GameObject.Instantiate(cellParent, pos, transform.rotation, transform);
                    g.GetComponent<SpriteRenderer>().color = cellArray[i, j] == 1 ? Color.red : Color.black;
                    objectsArray[i, j] = g;
                }

            }
    }

    void GameLoopThreads()
    {

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

    void A(Vector2 start, Vector2 end, int[,] temp)
    {
        for (int i = (int)start.x; i < end.x; i++)
            for (int j = (int)start.y; j < end.y; j++)
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
        if (Random.Range(0, 100) < mutationPercentage)
            return Mathf.Abs(cellArray[x, y] - 1);
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

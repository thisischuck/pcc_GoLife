using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;

public class GameOfLife : MonoBehaviour
{
    public Vector2 gridSize;
    public bool isDrawing;
    [Range(0, 1)]
    public float mutationPercentage;

    private int gridWitdh;
    private int gridHeight;

    private int frameCount = 0;
    private float timePassed = 0;

    #region ComputeShader

    public ComputeShader lifeShader;
    public ComputeShader startShader;
    public ComputeShader copyShader;
    public RenderTexture result;
    public RenderTexture start;

    public GameObject plane;

    int k;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        k = lifeShader.FindKernel("CSMain");
        gridWitdh = (int)gridSize.x;
        gridHeight = (int)gridSize.y;
        CreateGame();
    }

    RenderTexture SetupRenderTexture()
    {
        RenderTexture t;
        t = new RenderTexture(gridWitdh, gridHeight, 1);
        t.enableRandomWrite = true;
        t.filterMode = FilterMode.Point;
        t.Create();

        return t;
    }

    void CreateGame()
    {
        lifeShader.SetFloats("size", gridWitdh, gridWitdh);
        start = SetupRenderTexture();

        startShader.SetTexture(k, "Result", start);
        startShader.Dispatch(k, gridWitdh / 8, gridHeight / 8, 1);
        plane.GetComponent<MeshRenderer>().material.mainTexture = start;

        result = SetupRenderTexture();
        lifeShader.SetTexture(k, "Start", start);
        lifeShader.SetTexture(k, "Result", result);
        lifeShader.SetFloat("mutPercentage", mutationPercentage);

        copyShader.SetTexture(
            k,
            "Dest",
            start
        );

        copyShader.SetTexture(
            k,
            "Source",
            result
        );
    }

    void DrawTexture()
    {
        lifeShader.Dispatch(k, gridWitdh / 8, gridHeight / 8, 1);

        plane.GetComponent<MeshRenderer>().material.mainTexture = result;

        copyShader.Dispatch(k, gridWitdh / 8, gridHeight / 8, 1);
    }

    void Update()
    {
        if (isDrawing)
            DrawTexture();

        frameCount++;
        timePassed += Time.deltaTime;
        if (timePassed > 1)
        {
            //Debug.Log("Framerate " + frameCount);
            frameCount = 0;
            timePassed = 0;
        }
    }

}
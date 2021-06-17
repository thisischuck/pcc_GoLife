using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;

public class GameOfLife : MonoBehaviour
{
    public Vector2 gridSize = new Vector2(15360, 8640);
    public bool isDrawing;
    [Range(0, 100)]
    public int mutationPercentage;

    private int gridWitdh;
    private int gridHeight;

    private int frameCount = 0;
    private float timePassed = 0;

    #region ComputeShader

    private ComputeShader[] lifeShaders;
    private ComputeShader[] startShaders;
    private ComputeShader[] copyShaders;
    public RenderTexture[] results;
    public RenderTexture[] starts;

    public List<GameObject> planes;

    int k;
    #endregion

    public ComputeShader lifeShader;
    public ComputeShader startShader;
    public ComputeShader copyShader;

    void Fill(ComputeShader[] array, int tag)
    {
        for (int i = 0; i < planes.Count; i++)
        {
            switch (tag)
            {
                case 0:
                    array[i] = (ComputeShader)Instantiate(startShader);
                    continue;
                case 1:
                    array[i] = (ComputeShader)Instantiate(lifeShader);
                    continue;
                case 2:
                    array[i] = (ComputeShader)Instantiate(copyShader);
                    continue;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        results = new RenderTexture[planes.Count];
        starts = new RenderTexture[planes.Count];
        lifeShaders = new ComputeShader[planes.Count];
        startShaders = new ComputeShader[planes.Count];
        copyShaders = new ComputeShader[planes.Count];

        Fill(startShaders, 0);
        Fill(lifeShaders, 1);
        Fill(copyShaders, 2);

        k = lifeShader.FindKernel("CSMain");
        gridWitdh = (int)gridSize.x;
        gridHeight = (int)gridSize.y;


        for (int i = 0; i < planes.Count; i++)
        {
            CreateGame(planes[i], i);
        }
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

    void CreateGame(GameObject p, int id)
    {

        lifeShaders[id].SetFloats("size", gridWitdh, gridWitdh);
        starts[id] = SetupRenderTexture();

        startShaders[id].SetTexture(k, "Result", starts[id]);
        startShaders[id].Dispatch(k, gridWitdh / 8, gridHeight / 8, 1);
        p.GetComponent<MeshRenderer>().material.mainTexture = starts[id];

        results[id] = SetupRenderTexture();
        lifeShaders[id].SetTexture(k, "Start", starts[id]);
        lifeShaders[id].SetTexture(k, "Result", results[id]);
    }

    void DrawTexture(GameObject p, int id)
    {
        lifeShaders[id].SetTexture(k, "Start", starts[id]);
        lifeShaders[id].Dispatch(k, gridWitdh / 8, gridHeight / 8, 1);

        p.GetComponent<MeshRenderer>().material.mainTexture = results[id];

        copyShaders[id].SetTexture(
            k,
            "Dest",
            starts[id]
        );

        copyShaders[id].SetTexture(
            k,
            "Source",
            results[id]
        );

        copyShaders[id].Dispatch(k, gridWitdh / 8, gridHeight / 8, 1);
    }

    void Update()
    {
        if (isDrawing)
        {
            for (int i = 0; i < planes.Count; i++)
            {
                DrawTexture(planes[i], i);
            }
        }

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
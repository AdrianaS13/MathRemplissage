using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casteljau : MonoBehaviour
{
    public Transform[] controlPoints;
    public int curveResolution = 45;

    public PointHandler pointHandler;
    private LineRenderer BezierLineRenderer;
    private LineRenderer controlLineRenderer;

    public Shader lineShader;
    public float stepSize = 0.01f;
    public float stepSizeChangeAmount = 0.001f; 


    // Start is called before the first frame update
    void Start()
    {

        GameObject controlLineObject = new GameObject("ControlLines");
        controlLineRenderer = controlLineObject.AddComponent<LineRenderer>();
        controlLineRenderer.startWidth = 0.3f; 
        controlLineRenderer.endWidth = 0.3f;

        GameObject lineObject = new GameObject("BezierCurve");
        BezierLineRenderer = lineObject.AddComponent<LineRenderer>();
        Material lineMaterial = new Material(lineShader);

        BezierLineRenderer.material = lineMaterial;
        controlLineRenderer.material = lineMaterial;

        BezierLineRenderer.widthCurve = AnimationCurve.Linear(0.9f, 0.5f, 1, 0.5f);
        BezierLineRenderer.textureMode = LineTextureMode.Tile;
        BezierLineRenderer.numCapVertices = 10;
        BezierLineRenderer.numCornerVertices = 10;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            stepSize += stepSizeChangeAmount;
            DrawBezierCurve();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            stepSize -= stepSizeChangeAmount;
            stepSize = Mathf.Max(stepSize, 0.001f);
            DrawBezierCurve();
        }
    }

    public void DrawBezierCurve()
    {
        if (pointHandler.points.Count < 2)
        {
            Debug.LogError("At least two control points are required for drawing a Bezier curve.");
            return;
        }

        int numPoints = Mathf.CeilToInt(1f / stepSize);

        List<Vector3> curvePoints = new List<Vector3>();


        for (int i = 0; i < numPoints; i++)
        {
            float t = i * stepSize;
            Vector3 point = CalculateBezierPoint(t, pointHandler.points);
            curvePoints.Add(point);

        }

        BezierLineRenderer.positionCount = curvePoints.Count;
        BezierLineRenderer.startColor = pointHandler.currentColor;
        BezierLineRenderer.endColor = pointHandler.currentColor;
        BezierLineRenderer.SetPositions(curvePoints.ToArray());
        BezierLineRenderer.positionCount = numPoints;
    }

    private Vector3 CalculateBezierPoint(float t, List<GameObject> controlPoints)
    {
        int numPoints = controlPoints.Count;
        int lastIndex = numPoints - 1;

        List<Vector3> controlPositions = new List<Vector3>();

        for (int i = 0; i < numPoints; i++)
        {
            controlPositions.Add(controlPoints[i].transform.position);
        }

        for (int j = 1; j < numPoints; j++)
        {
            for (int k = 0; k < numPoints - j; k++)
            {
                controlPositions[k] = controlPositions[k] * (1 - t) + controlPositions[k + 1] * t;
            }
        }
        return controlPositions[0];
    }

    public void clearCurve()
    {
        LineRenderer[] lineRenderers = FindObjectsOfType<LineRenderer>();

        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            lineRenderer.positionCount = 0;
        }
    }
}

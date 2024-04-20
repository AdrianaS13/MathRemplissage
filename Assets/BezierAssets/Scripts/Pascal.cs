using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pascal : MonoBehaviour
{
    public PointHandler controlPoints;
    public int resolution = 30;

    public Shader lineShader;
    public float stepSize = 0.01f;
    public float stepSizeChangeAmount = 0.001f;
    public NewFill drawable;

    public bool pascal = false;

    public void ActivatePascal()
    {
        pascal = true;
    }
    public void DrawCurve(List<GameObject> controlPointsList, GameObject parent)
    {
        if (controlPointsList.Count < 2)
        {
            Debug.LogError("Se necesitan al menos 2 puntos de control para una curva de Bezier.");
            return;
        }

        List<Vector3> curvePoints = new List<Vector3>();

        for (int j = 0; j <= resolution; j++)
        {
            float t = (float)j / resolution;

            Vector3 point = CalculateBezierPointUsingPascal(t, controlPointsList);
            curvePoints.Add(point);
        }

        GameObject bezierCurveObj = new GameObject("PascalBezierCurve");
        bezierCurveObj.transform.SetParent(parent.transform);

        LineRenderer lineRenderer = bezierCurveObj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = curvePoints.Count;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = controlPoints.currentColor;
        lineRenderer.endColor = controlPoints.currentColor;
        lineRenderer.SetPositions(curvePoints.ToArray());


        Material lineMaterial = new Material(lineShader);

        lineRenderer.material = lineMaterial;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.numCapVertices = 10;
        lineRenderer.numCornerVertices = 10;

        drawable.paintInPixels(curvePoints);
    }

    private Vector3 CalculateBezierPointUsingPascal(float t, List<GameObject> points)
    {
        int numPoints = points.Count;
        int lastIndex = numPoints - 1;

        List<Vector3> controlPositions = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            controlPositions.Add(points[i].transform.position);
        }

        List<int> coefficients = CalculateBinomialCoefficients(numPoints - 1);

        Vector3 result = Vector3.zero;

        for (int i = 0; i < numPoints; i++)
        {
            float term = coefficients[i] * Mathf.Pow(t, i) * Mathf.Pow(1 - t, numPoints - 1 - i);
            result += controlPositions[i] * term;
        }

        return result;
    }

    private List<int> CalculateBinomialCoefficients(int n)
    {
        List<int> coefficients = new List<int>();
        coefficients.Add(1);

        for (int i = 1; i <= n; i++)
        {
            int nextCoefficient = coefficients[i - 1] * (n - i + 1) / i;
            coefficients.Add(nextCoefficient);
        }

        return coefficients;
    }

}

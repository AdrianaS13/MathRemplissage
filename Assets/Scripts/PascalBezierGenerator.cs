using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PascalBezierGenerator : MonoBehaviour
{
    public Drawing drawingScript; // R�f�rence au script Drawing
    public List<Vector2Int> lastPolygonPixelVertices; // Derniers points de polygones
    public List<List<Vector2Int>> polygons; // Liste des polygones

    public List<Vector2> controlPoints = new List<Vector2>(); // Points de contr�le de la courbe de B�zier
    public List<List<int>> pascalTriangle = new List<List<int>>(); // Triangle de Pascal

    public LineRenderer lineRenderer; // R�f�rence au LineRenderer pour tracer la courbe de B�zier
    public float step = 0.01f; // Pas pour tracer la courbe de B�zier

    void Start()
    {
        drawingScript = GetComponent<Drawing>(); // R�cup�rer le script Drawing attach� � ce GameObject

        // Initialiser les listes de points de polygones et de polygones en utilisant les donn�es du script Drawing
        lastPolygonPixelVertices = drawingScript.lastPolygonPixelVertices;
        polygons = drawingScript.polygons;

        // G�n�rer le triangle de Pascal
        GeneratePascalTriangle();

        // R�cup�rer le LineRenderer attach� � ce GameObject
        lineRenderer = GetComponent<LineRenderer>();
    }

    void GeneratePascalTriangle()
    {
        // Initialisation du triangle de Pascal avec les valeurs de la premi�re ligne
        pascalTriangle.Add(new List<int> { 1 });

        // G�n�rer les lignes suivantes du triangle de Pascal
        for (int i = 1; i < controlPoints.Count; i++)
        {
            List<int> currentRow = new List<int>();
            List<int> previousRow = pascalTriangle[i - 1];

            // La premi�re valeur de chaque ligne est 1
            currentRow.Add(1);

            // Calculer les valeurs suivantes en utilisant la formule de Pascal
            for (int j = 1; j < i; j++)
            {
                int value = previousRow[j - 1] + previousRow[j];
                currentRow.Add(value);
            }

            // La derni�re valeur de chaque ligne est 1
            currentRow.Add(1);

            // Ajouter la ligne actuelle au triangle de Pascal
            pascalTriangle.Add(currentRow);
        }
    }

    Vector2 CalculateBezierPoint(float t)
    {
        int n = controlPoints.Count - 1;
        Vector2 point = Vector2.zero;
        for (int i = 0; i <= n; i++)
        {
            int coeff = pascalTriangle[n][i];
            float term = coeff * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i);
            point += controlPoints[i] * term;
        }
        return point;
    }

    void Update()
    {
        // G�rer le clic pour g�n�rer la courbe de B�zier
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsClickInControlPolygon(mousePosition))
            {
                DrawBezierCurve();
            }
        }

        // Mettre � jour le trac� de la courbe de B�zier si les points de contr�le sont modifi�s
        if (controlPoints.Count > 0)
        {
            DrawBezierCurve();
        }
    }

    bool IsClickInControlPolygon(Vector2 mousePosition)
    {
        // Impl�mentez ici la logique pour v�rifier si le clic est dans le polygone des points de contr�le
        // Vous pouvez utiliser des techniques de g�om�trie pour d�terminer si le point cliqu� est � l'int�rieur du polygone
        return true;
    }

    void DrawBezierCurve()
    {
        lineRenderer.positionCount = 0; // R�initialiser les positions du LineRenderer

        // Dessiner la courbe de B�zier en utilisant CalculateBezierPoint() pour chaque valeur de t
        for (float t = 0; t <= 1; t += step)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, CalculateBezierPoint(t));
        }
    }
}

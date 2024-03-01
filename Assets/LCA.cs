using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LCA : MonoBehaviour
{
    [SerializeField] private Drawing drawingScript;
    public Color fillColor ;

   private class Edge
{
    public float yMin, yMax, xWithYMin, slopeInverse;

    public Edge(Vector2 start, Vector2 end)
    {
        // Initialiser yMin et yMax pour s'assurer que yMin est toujours le plus petit
        yMin = Mathf.Min(start.y, end.y);
        yMax = Mathf.Max(start.y, end.y);

        // Identifier le point qui a le yMin et initialiser xWithYMin en conséquence
        if (start.y == yMin)
        {
            xWithYMin = start.x;
        }
        else
        {
            xWithYMin = end.x;
        }

        // Calculer la pente inverse, en traitant les arêtes verticales
        if (Mathf.Approximately(start.x, end.x))
        {
            // La pente inverse d'une arête verticale peut être définie comme 0 ou une autre valeur spéciale
            // Cela nécessite une gestion spécifique lors du calcul des intersections
            slopeInverse = float.PositiveInfinity; 
        }
        else
        {
            slopeInverse = (end.x - start.x) / (end.y - start.y);
        }
    }
}


    private List<Edge> CreateEdgeTable(List<Vector2> polyVertices)
    {
        List<Edge> edgeTable = new List<Edge>();

        for (int i = 0; i < polyVertices.Count; i++)
        {
            Vector2 start = polyVertices[i];
            Vector2 end = polyVertices[(i + 1) % polyVertices.Count];

            if (Mathf.Abs(start.y - end.y) > Mathf.Epsilon) // Ignorer les arêtes horizontales avec une précision améliorée
            {
                Edge edge = new Edge(start, end);
                edgeTable.Add(edge);
            }
        }

        edgeTable.Sort((edge1, edge2) => edge1.yMin.CompareTo(edge2.yMin));
        return edgeTable;
    }

    public void FillPolygon()
    {
        Debug.Log($"Starting FillPolygon with {drawingScript.polygonVertices.Count} vertices and color {fillColor}.");
        
        if (drawingScript == null || drawingScript.polygonVertices.Count < 3) return; // Assurer un polygone valide

        List<Edge> SI = CreateEdgeTable(drawingScript.polygonVertices);
        List<Edge> LCA = new List<Edge>();

    // Trouver Y min et Y max du polygone
        float yMin = drawingScript.polygonVertices.Min(v => v.y);
        float yMax = drawingScript.polygonVertices.Max(v => v.y);
        // Pour chaque ligne de balayage Y
        for (float y = yMin; y <= yMax; y++)
        {
            // Gérer les entrées dans LCA à partir de SI
            LCA.Clear(); // Assurez-vous de réinitialiser LCA pour chaque nouvelle ligne de balayage
            LCA.AddRange(SI.Where(edge => edge.yMin <= y && edge.yMax > y)); // Modification ici

            // Trier LCA par xWithYMin croissant
            LCA.Sort((edge1, edge2) => edge1.xWithYMin.CompareTo(edge2.xWithYMin));

            // Mise à jour des positions X en utilisant slopeInverse
            foreach (var edge in LCA)
            {
                if (edge.yMin < y) // Mise à jour si ce n'est pas le premier y de l'arête
                {
                    edge.xWithYMin += edge.slopeInverse;
                }
            }

            // Dessiner entre les points d'intersection sur la ligne de balayage
            for (int i = 0; i < LCA.Count; i += 2)
            {
                if (i + 1 < LCA.Count) // Assurez-vous qu'il y a une paire
                {
                    Vector2 start = new Vector2(LCA[i].xWithYMin, y);
                    Vector2 end = new Vector2(LCA[i + 1].xWithYMin, y);
                    DrawFillBetweenPoints(start, end, fillColor);
                }
            }
        }
    }

    private void DrawFillBetweenPoints(Vector2 start, Vector2 end, Color color)
    {
        // Convertir les coordonnées du monde en coordonnées de texture
        Vector2 startPixel = drawingScript.WorldToPixelCoordinates(start);
        Vector2 endPixel = drawingScript.WorldToPixelCoordinates(end);

        // Calculer la direction et la distance entre les points
        Vector2 direction = endPixel - startPixel;
        float distance = direction.magnitude; // Utilisation de magnitude pour obtenir la distance entre les points

        // Dessiner un point pour chaque pas de la distance
        for (float i = 0; i <= distance; i++)
        {
            float t = i / distance;
            // Effectuer l'interpolation en utilisant une méthode plus précise pour obtenir des valeurs flottantes
            Vector2 interpolatedPoint = Vector2.LerpUnclamped(startPixel, endPixel, t);
            // Convertir le résultat de l'interpolation en Vector2Int pour le dessin
            Vector2Int pixelPoint = new Vector2Int(Mathf.RoundToInt(interpolatedPoint.x), Mathf.RoundToInt(interpolatedPoint.y));
            drawingScript.drawable_texture.SetPixel(pixelPoint.x, pixelPoint.y, color);
        }

        drawingScript.drawable_texture.Apply();
    }


    public void OnClickFillPolygon()
    {
        FillPolygon();
    }
}

using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PointHandler : MonoBehaviour
{
    public GameObject pointPrefab; // Prefab pour l'objet point
    public List<GameObject> points = new List<GameObject>(); // Liste pour stocker les points
    private List<GameObject> lines = new List<GameObject>(); // Liste pour stocker les objets ligne (polygones)
    private bool drawing = false; // Indique si le dessin est en cours
    public Color currentColor = Color.red; // Couleur actuellement sélectionnée

    private bool isCheckingPolygon = false; // Indique si la vérification des polygones est active
    public List<List<GameObject>> courbes = new List<List<GameObject>>();

    public Casteljau decasteljauScript;
    public Pascal pascalScript;
    public Fill fill;
    public bool clearOne = false;

    
    

    void Update()
    {
        // Vérifier si le clic gauche de la souris est enfoncé et si le dessin est en cours et que la souris n'est pas sur un objet UI
        if (Input.GetMouseButtonDown(0) && drawing && !IsPointerOverUIObject())
        {
            // Obtenir la position de la souris dans l'espace du monde
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // S'assurer que la coordonnée z est 0 pour l'espace 2D

            // Instancier un objet point à la position de la souris
            GameObject point = Instantiate(pointPrefab, mousePos, Quaternion.identity);
            point.tag = "controlPoint";
            point.GetComponent<Renderer>().material.color = currentColor; // Définir la couleur du point
            points.Add(point); // Ajouter le point à la liste
        }

        // Vérifier si le clic droit de la souris est enfoncé et si le dessin est en cours
        if (Input.GetMouseButtonDown(1) && drawing)
        {
            drawing = false; // Arrêter le dessin
            List<GameObject> currentPoints = new List<GameObject>(points);
            ConnectPoints(currentPoints); // Connecter les points pour former un polygone
            courbes.Add(currentPoints);
        }

        // Vérifier si la vérification des polygones est active et si le clic gauche de la souris est enfoncé
        if (isCheckingPolygon && Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        {
            isCheckingPolygon = false;
            GameObject insidePolygon = IsInsidePolygon();
            if (insidePolygon != null)
            {
                Debug.Log("Clic à l'intérieur du polygone : " + insidePolygon.name);
                // Faites quelque chose avec l'objet line (polygone) trouvé

                // Get the points of the clicked polygon
                List<GameObject> polygonPoints = new List<GameObject>();
                foreach (Transform child in insidePolygon.transform)
                {
                    polygonPoints.Add(child.gameObject);
                }

                if (decasteljauScript.decasteljau)
                {
                    decasteljauScript.DrawBezierCurve(polygonPoints, insidePolygon);
                    decasteljauScript.decasteljau = false;
                    print("casteljau function worked");

                } else if (pascalScript.pascal)
                {
                    pascalScript.DrawCurve(polygonPoints, insidePolygon);
                    pascalScript.pascal = false;
                    print("pascal function worked");
                } else if(clearOne)
                {
                    Destroy(insidePolygon);
                    clearOne = false;
                    lines.Remove(insidePolygon); 
                    //need to add : delete the polygon from 'courbes'
                    print("cleared one polygon");
                } 
            }
            else
            {
                Debug.Log("Clic à l'extérieur de tous les polygones.");
            }
        }
    }
    public void ClearOne()
    {
        clearOne = true;
    }

    
    // Méthode pour connecter les points pour former un polygone
    public void ConnectPoints(List<GameObject> currentPoints)
    {
        // S'assurer qu'il y a au moins 3 points pour former un polygone
        if (currentPoints.Count < 3)
        {
            Debug.LogWarning("Pas assez de points pour former un polygone.");
            return;
        }

        // Créer un nouvel objet pour contenir les lignes (polygone)
        GameObject polygonObj = new GameObject("Polygon");
        lines.Add(polygonObj);

        // Ajouter un composant LineRenderer pour dessiner les lignes
        LineRenderer lineRenderer = polygonObj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = currentPoints.Count; // Définir le nombre de positions
        lineRenderer.startWidth = 0.1f; // Définir la largeur de la ligne
        lineRenderer.endWidth = 0.1f;
        lineRenderer.loop = true; // Relier le dernier point au premier point

        // Définir la couleur de la ligne pour correspondre à la couleur des points
        lineRenderer.material.color = currentColor;

        // Définir les positions pour le LineRenderer
        for (int i = 0; i < currentPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, currentPoints[i].transform.position);
            Debug.Log("punto " + i + " : " + currentPoints[i].transform.position);
            currentPoints[i].transform.parent = polygonObj.transform; // Faire du point un enfant de l'objet ligne (polygone)
            fill.paintInPixel(lineRenderer, i);
        }

        // Effacer la liste des points pour la prochaine session de dessin
        points.Clear();
    }

    // Méthode pour effacer tous les points et polygones
    public void Clear()
    {
        foreach (GameObject point in points)
        {
            Destroy(point);
        }
        points.Clear();

        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        courbes.Clear();
    }

    // Méthode pour définir la couleur de dessin actuelle
    public void SetColorRed()
    {
        currentColor = Color.red;
        drawing = true; // Commencer le dessin lorsque la couleur est sélectionnée
    }

    public void SetColorGreen()
    {
        currentColor = Color.green;
        drawing = true; // Commencer le dessin lorsque la couleur est sélectionnée
    }

    public void SetColorBlue()
    {
        currentColor = Color.blue;
        drawing = true; // Commencer le dessin lorsque la couleur est sélectionnée
    }

    public void SetColorBlack()
    {
        currentColor = Color.black;
        drawing = true;
    }

    // Méthode pour vérifier si le pointeur de la souris est sur un objet UI
    bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    // Méthode pour activer la vérification des polygones
    public void StartCheckingPolygon()
    {
        isCheckingPolygon = true;
    }

    // Méthode pour vérifier si le clic gauche de la souris est à l'intérieur d'un polygone existant
    public GameObject IsInsidePolygon()
    {
        // Obtenir la position du clic de la souris dans l'espace du monde
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // S'assurer que la coordonnée z est 0 pour l'espace 2D

        foreach (GameObject lineObj in lines)
        {
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                Vector2[] polygon = new Vector2[lineRenderer.positionCount];
                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    polygon[i] = new Vector2(lineRenderer.GetPosition(i).x, lineRenderer.GetPosition(i).y);
                }

                if (IsPointInPolygon(new Vector2(mousePos.x, mousePos.y), polygon))
                {
                    return lineObj; // Le clic est à l'intérieur d'un polygone existant
                }
            }
        }
        return null; // Le clic est à l'extérieur de tous les polygones
    }

    // Méthode pour vérifier si un point est à l'intérieur d'un polygone
    private bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int polygonLength = polygon.Length;
        bool inside = false;

        for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
        {
            if (((polygon[i].y <= point.y && point.y < polygon[j].y) || (polygon[j].y <= point.y && point.y < polygon[i].y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}

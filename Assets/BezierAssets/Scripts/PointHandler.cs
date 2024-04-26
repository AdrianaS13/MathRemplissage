using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;

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
    public Drawing drawable;
    public bool clearOne = false;

    public bool isLinking = false;
    public int linkType;

    public List<Vector3> polygonPoints = new List<Vector3>();

    private void Awake()
    {
        points.Clear();
        lines.Clear();
        polygonPoints.Clear();
    }
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
            //obtainPoints(currentPoints);
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

                } 
                else if (pascalScript.pascal)
                {
                    pascalScript.DrawCurve(polygonPoints, insidePolygon);
                    pascalScript.pascal = false;
                    print("pascal function worked");
                }
                else if (isLinking)
                {
                    if(linkType == 0)
                    {
                        C0Link(polygonPoints);
                    }
                    else if(linkType == 1)
                    {
                        C1Link(polygonPoints);
                    }
                    else if(linkType == 2)
                    {
                        C2Link(polygonPoints);
                    }
                    isLinking = false;
                    drawing = true;
                }
                else if(clearOne)
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
    public void obtainPoints(List<GameObject> currentPoints)
    {
        List<Vector2Int> vector2IntList = new List<Vector2Int>();

        foreach (GameObject obj in currentPoints)
        {
            Vector3 position = obj.transform.position;
            Vector2Int vector2IntPosition = new Vector2Int((int)position.x, (int)position.y);
            vector2IntList.Add(vector2IntPosition);
        }
       // drawable.drawEverything(vector2IntList);
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
            currentPoints[i].transform.parent = polygonObj.transform; // Faire du point un enfant de l'objet ligne (polygone)

            polygonPoints.Add(currentPoints[i].transform.position);
        }
        drawable.paintPolygonsInPixels(polygonPoints);
        // Effacer la liste des points pour la prochaine session de dessin
        points.Clear();
        polygonPoints.Clear();
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

    // Méthode pour le raccordement C0
    public void C0Link(List<GameObject> polygonPoints)
    {
        // Créer un nouveau point au même emplacement que le dernier point du polygone existant
        GameObject newPoint = Instantiate(pointPrefab, polygonPoints[polygonPoints.Count - 1].transform.position, Quaternion.identity);
        newPoint.tag = "controlPoint"; // Ajouter le tag "controlPoint"
        newPoint.GetComponent<Renderer>().material.color = currentColor; // Définir la couleur du nouveau point
        points.Add(newPoint); // Ajouter le point à la liste des points
    }

    // Méthode pour le raccordement C1
    public void C1Link(List<GameObject> polygonPoints)
    {
        // Créer deux nouveaux points
        GameObject newPoint1 = Instantiate(pointPrefab, polygonPoints[polygonPoints.Count - 1].transform.position, Quaternion.identity);
        GameObject newPoint2 = Instantiate(pointPrefab, CalculateC1Position(polygonPoints), Quaternion.identity);

        newPoint1.tag = "controlPoint"; // Ajouter le tag "controlPoint"
        newPoint2.tag = "controlPoint"; // Ajouter le tag "controlPoint"

        newPoint1.GetComponent<Renderer>().material.color = currentColor; // Définir la couleur du premier nouveau point
        newPoint2.GetComponent<Renderer>().material.color = currentColor; // Définir la couleur du deuxième nouveau point

        points.Add(newPoint1); // Ajouter le premier nouveau point à la liste des points
        points.Add(newPoint2); // Ajouter le deuxième nouveau point à la liste des points
    }

    // Méthode pour le raccordement C2
    public void C2Link(List<GameObject> polygonPoints)
    {
        // Créer trois nouveaux points
        GameObject newPoint1 = Instantiate(pointPrefab, polygonPoints[polygonPoints.Count - 1].transform.position, Quaternion.identity);
        GameObject newPoint2 = Instantiate(pointPrefab, CalculateC1Position(polygonPoints), Quaternion.identity);
        GameObject newPoint3 = Instantiate(pointPrefab, CalculateC2Position(polygonPoints), Quaternion.identity);

        newPoint1.tag = "controlPoint"; // Ajouter le tag "controlPoint"
        newPoint2.tag = "controlPoint"; // Ajouter le tag "controlPoint"
        newPoint3.tag = "controlPoint"; // Ajouter le tag "controlPoint"

        newPoint1.GetComponent<Renderer>().material.color = currentColor; // Définir la couleur du premier nouveau point
        newPoint2.GetComponent<Renderer>().material.color = currentColor; // Définir la couleur du deuxième nouveau point
        newPoint3.GetComponent<Renderer>().material.color = currentColor; // Définir la couleur du troisième nouveau point

        points.Add(newPoint1); // Ajouter le premier nouveau point à la liste des points
        points.Add(newPoint2); // Ajouter le deuxième nouveau point à la liste des points
        points.Add(newPoint3); // Ajouter le troisième nouveau point à la liste des points
    }

    // Méthode pour calculer la position du deuxième point pour le raccordement C1
    private Vector3 CalculateC1Position(List<GameObject> polygonPoints)
    {
        // Obtenir la position du dernier point du polygone
        Vector3 lastPointPosition = polygonPoints[polygonPoints.Count - 1].transform.position;

        // Obtenir la position du deuxième dernier point du polygone
        Vector3 secondLastPointPosition = polygonPoints[polygonPoints.Count - 2].transform.position;

        // Calculer la position du deuxième nouveau point
        // P1' = P0' + (Pn - Pn-1) -> Miroir du point Pn-1
        Vector3 c1Position = lastPointPosition + (lastPointPosition - secondLastPointPosition);

        return c1Position;
    }

    // Méthode pour calculer la position du troisième point pour le raccordement C2
    private Vector3 CalculateC2Position(List<GameObject> polygonPoints)
    {
        // Obtenir la position du dernier point du polygone
        Vector3 lastPointPosition = polygonPoints[polygonPoints.Count - 1].transform.position;

        // Obtenir la position du deuxième dernier point du polygone
        Vector3 secondLastPointPosition = polygonPoints[polygonPoints.Count - 2].transform.position;

        // Obtenir la position du troisième dernier point du polygone
        Vector3 thirdLastPointPosition = polygonPoints[polygonPoints.Count - 3].transform.position;

        // Calculer la position du troisième nouveau point
        // P2' = Pn+2 + 2*(P1' - Pn-1) -> Miroir des points Pn-1 et Pn-2
        Vector3 c2Position = lastPointPosition + (lastPointPosition - secondLastPointPosition) + (lastPointPosition - 2 * secondLastPointPosition + thirdLastPointPosition);

        return c2Position;
    }

    public void SetLinkType(int type)
    {
        isLinking = true;
        linkType = type;
    }
}

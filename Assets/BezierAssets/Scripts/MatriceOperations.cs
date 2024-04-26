using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class MatriceOperations : MonoBehaviour
{
    public GameObject menuCanvas;
    public Button translateButton;
    public Button rotateButton;

    private Vector3 initialMousePosition;
    private Vector3 initialGameObjectPosition;

    public bool translating = false;
    private bool isDragging = false;

    private GameObject selectedObject;
    private Vector3 lastMousePosition;

    public float translationSpeed = 5f;
    private GameObject translationIconInstance;
    public GameObject translationIconPrefab; 
    public Vector3 translationIconOffset = new Vector3(0.5f, 0.5f, 0.5f);

    public PointHandler pointHandler;

    private new LineRenderer renderer;
    private List<GameObject> newPoly = new List<GameObject>();

    public Casteljau decasteljauScript;
    public Pascal pascalScript;

    private GameObject DecasteljauCurveObj;
    private void Start()
    {
        translateButton.onClick.AddListener(StartTranslation);
        //rotateButton.onClick.AddListener(StartRotation);
        HideMenu();
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("controlPoint"))
                {
                    ShowMenu();
                    initialMousePosition = Input.mousePosition;
                    initialGameObjectPosition = hit.transform.position;
                    selectedObject = hit.collider.gameObject;
                    lastMousePosition = Input.mousePosition;
                    isDragging = true;
                } 
            }

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (!Physics.Raycast(ray, out hit))
                {
                    HideMenu();
                }
            }
        }

        if (translating)
        {
            pointHandler.drawable.ResetCanvas();
            if (Input.GetMouseButton(0) && selectedObject != null)
            {
                Vector3 currentMousePosition = Input.mousePosition;
                Vector3 mouseDelta = currentMousePosition - lastMousePosition;

                selectedObject.transform.Translate(mouseDelta * Time.deltaTime);

                lastMousePosition = currentMousePosition;

                UpdatePolygon(selectedObject);

            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging && selectedObject != null)
            {
                Vector3 offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.ScreenToWorldPoint(initialMousePosition);
                selectedObject.transform.position = initialGameObjectPosition + offset;
            }

            // Translation logic using keyboard keys 
            Vector3 translationVector = Vector3.zero;
            if (Input.GetKey(KeyCode.A))
            {
                translationVector += Vector3.left;
                UpdatePolygon(selectedObject);
            }
            if (Input.GetKey(KeyCode.S))
            {
                translationVector += Vector3.down;
                UpdatePolygon(selectedObject);
            }
            if (Input.GetKey(KeyCode.D))
            {
                translationVector += Vector3.right;
                UpdatePolygon(selectedObject);
            }
            if (Input.GetKey(KeyCode.W))
            {
                translationVector += Vector3.up;
                UpdatePolygon(selectedObject);
            }

            if (translationVector.magnitude > 1f)
            {
                translationVector.Normalize();
            }

            selectedObject.transform.Translate(translationVector * translationSpeed * Time.deltaTime);

            // Show translation icon above the selected object
            if (translationIconPrefab != null && translationIconInstance == null)
            {
                translationIconInstance = Instantiate(translationIconPrefab, selectedObject.transform.position /*+ translationIconOffset*/, Quaternion.identity);
            }
            else if (translationIconInstance != null)
            {
                translationIconInstance.transform.position = selectedObject.transform.position /*+ translationIconOffset*/;
            }
        }

        else
        {
            // Destroy translation icon when not in translation mode or no selected object
            if (translationIconInstance != null)
            {
                Destroy(translationIconInstance);
                translationIconInstance = null;
            }
        }
    }

    private void UpdatePolygon(GameObject AselectedObject)
    {
        //pour modifier les lignes du polygone
        newPoly = FindPolygon(AselectedObject);
        renderer = AselectedObject.transform.parent.GetComponent<LineRenderer>();

        renderer.positionCount = newPoly.Count;
        for (int i = 0; i < newPoly.Count; i++)
        {
            renderer.SetPosition(i, newPoly[i].transform.position);
            newPoly[i].transform.parent = selectedObject.transform.parent.transform;
        }
        renderer.sortingOrder = 1; 

        //modifier la courbe
        DecasteljauCurveObj = DecasteljauCurveIsPresent(AselectedObject.transform.parent.gameObject);
        if (DecasteljauCurveObj != null)
        {
            if(DecasteljauCurveObj.name == "CasteljauBezierCurve")
            {
                print("curve present");
                decasteljauScript.UpdateDecasteljau(newPoly, DecasteljauCurveObj);
                print("casteljau function worked");
            }else if (DecasteljauCurveObj.name == "PascalBezierCurve")
            {
                print("curve present");
                pascalScript.UpdatePascal(newPoly, DecasteljauCurveObj);
                print("casteljau function worked");
            }
        }
        else
        {
            print("curve not present");
        }
        
    }

    private GameObject DecasteljauCurveIsPresent(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            // Check if the name of the current child matches the specified name
            if (child.gameObject.name == "CasteljauBezierCurve" || child.gameObject.name == "PascalBezierCurve")
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public List<GameObject> FindPolygon(GameObject point)
    {
        foreach (List<GameObject> polygon in pointHandler.courbes)
        {
            foreach (GameObject polygonPoint in polygon)
            {
                if (polygonPoint == point)
                {
                    return polygon;
                }
            }
        }
        return null; 
    }

    private void ShowMenu()
    {
        menuCanvas.SetActive(true);
    }

    private void HideMenu()
    {
        menuCanvas.SetActive(false);
        translating= false;
    }

    public void StartTranslation()
    {
        translating=true;
        // Implement translation logic here
        Debug.Log("Translation started");
    }

    private void StartRotation()
    {
        // Implement rotation logic here
        Debug.Log("Rotation started");
    }
}

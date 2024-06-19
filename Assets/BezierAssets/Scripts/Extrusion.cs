using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Extrusion : MonoBehaviour
{
    public List<Vector3> curvePoints; // Points de la courbe de Bézier en 3D
    public float height = -1.0f; // Hauteur d'extrusion
    public float scale = 1.0f; // Coefficient d'agrandissement ou de réduction
    private Mesh mesh;
    private MeshRenderer meshRenderer;


    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        GenerateMesh();
    }
    void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (Vector3 point in curvePoints)
        {
            vertices.Add(new Vector3(point.x, point.y, 0)); 
            vertices.Add(new Vector3(point.x * scale, point.y * scale, height)); 
        }

        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            int baseIndex = i * 2;

            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);

            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex + 2);
        }

        // Generate triangles for the top and bottom faces
        // Uncomment and implement AddCap if needed
        // AddCap(triangles, vertices, true); // Top face
        // AddCap(triangles, vertices, false); // Bottom face

        // Apply vertices and triangles to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.ManuallyRecalculateNormals();

    }

    /* void AddCap(List<int> triangles, List<Vector3> vertices, bool isTop)
    {
        int startIndex = isTop ? 1 : 0;
        int step = 2;

        for (int i = 0; i < curvePoints.Count - 2; i++)
        {
            triangles.Add(startIndex);
            triangles.Add(startIndex + step * (i + 1));
            triangles.Add(startIndex + step * (i + 2));
        }
    } */

    // Méthode pour mettre à jour l'extrusion 
    public void UpdateExtrusion(List<Vector3> newCurvePoints, Color currentColor, Transform parent)
    {
        curvePoints = new List<Vector3>(newCurvePoints); 

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        //meshRenderer.material.color = currentColor;

        if (PointHandler.setMaterialWood)
        {
            Texture2D texture = Resources.Load<Texture2D>("wood");
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture;
            meshRenderer.material = material;
        } else if (PointHandler.setMaterialMetal)
        {
            Texture2D texture = Resources.Load<Texture2D>("metal");
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture;
            meshRenderer.material = material;
        } else
        {
            meshRenderer.material.color = currentColor;
        }
        ConfigureLighting();

        transform.SetParent(parent);

        GenerateMesh();
    }

    void ConfigureLighting()
    {
        // Crear una luz direccional
        GameObject lightGameObject = new GameObject("Directional Light");
        Light lightComp = lightGameObject.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.color = Color.white;
        lightComp.intensity = 1.0f;

        // Configurar la posición y rotación de la luz
        lightGameObject.transform.position = new Vector3(0, 10, 0);
        lightGameObject.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Opcional: Configurar más luces y parámetros de iluminación según sea necesario
    }
}

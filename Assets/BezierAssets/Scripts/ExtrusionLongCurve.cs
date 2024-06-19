using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ExtrusionLongCurve : MonoBehaviour
{
    public int segmentCount = 36; // Número de segmentos alrededor de la curva 3D
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ExtrudeAlongCurve(List<Vector3> shape, List<Vector3> path, Transform parent, Color currentColor)
    {
        if (shape == null || shape.Count < 2 || path == null || path.Count < 2)
        {
            Debug.LogError("La lista de puntos de la forma o del camino está vacía o tiene menos de dos puntos.");
            return;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        transform.SetParent(parent);
        meshFilter.mesh = CreateExtrudedMesh(shape, path, segmentCount, currentColor);
    }
    public Mesh CreateExtrudedMesh(List<Vector3> shape, List<Vector3> path, int segments, Color currentColor)
    {
        int shapePointCount = shape.Count;
        int pathPointCount = path.Count;
        int verticesCount = shapePointCount * pathPointCount;

        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[(shapePointCount - 1) * (pathPointCount - 1) * 6];
        Vector2[] uvs = new Vector2[verticesCount];

        // Invert shape along the Y-axis
        for (int i = 0; i < shape.Count; i++)
        {
            shape[i] = new Vector3(shape[i].x, -shape[i].y, shape[i].z);
        }

        // Crear vértices
        for (int i = 0; i < pathPointCount; i++)
        {
            Vector3 pathPoint = path[i];
            Quaternion rotation = Quaternion.identity;
            if (i < pathPointCount - 1)
            {
                Vector3 direction = (path[i + 1] - path[i]).normalized;
                rotation = Quaternion.LookRotation(direction, Vector3.down);
            }
            else if (i > 0)
            {
                Vector3 direction = (path[i] - path[i - 1]).normalized;
                rotation = Quaternion.LookRotation(direction, Vector3.down);
            }

            for (int j = 0; j < shapePointCount; j++)
            {
                Vector3 shapePoint = shape[j];
                int vertexIndex = i * shapePointCount + j;
                vertices[vertexIndex] = pathPoint + rotation * shapePoint;
                uvs[vertexIndex] = new Vector2((float)j / (shapePointCount - 1), (float)i / (pathPointCount - 1));
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = new int[0]; // Initially, no triangles
        mesh.ManuallyRecalculateNormals();
        mesh.SetNormals(vertices);

        // Apply the new color to the material
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
        }
        else if (PointHandler.setMaterialMetal)
        {
            Texture2D texture = Resources.Load<Texture2D>("metal");
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture;
            meshRenderer.material = material;
        }
        else
        {
            meshRenderer.material.color = currentColor;
        }
        return mesh;
    }
    public Mesh CreateExtrudedMeshWithoutRotation(List<Vector3> shape, List<Vector3> path, int segments, Color currentColor)
    {
        int shapePointCount = shape.Count;
        int pathPointCount = path.Count;
        int verticesCount = shapePointCount * pathPointCount;

        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[(shapePointCount - 1) * (pathPointCount - 1) * 6];
        Vector2[] uvs = new Vector2[verticesCount];


        for (int i = 0; i < pathPointCount; i++)
        {
            Vector3 pathPoint = path[i];

            for (int j = 0; j < shapePointCount; j++)
            {
                Vector3 shapePoint = shape[j];
                int vertexIndex = i * shapePointCount + j;
                vertices[vertexIndex] = new Vector3(pathPoint.x + shapePoint.x, pathPoint.y + shapePoint.y, pathPoint.z);
                uvs[vertexIndex] = new Vector2((float)j / (shapePointCount - 1), (float)i / (pathPointCount - 1));
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = new int[0]; // Initially, no triangles
        mesh.ManuallyRecalculateNormals();
        mesh.SetNormals(vertices);

        // Apply the new color to the material
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        if (PointHandler.setMaterialWood)
        {
            Texture2D texture = Resources.Load<Texture2D>("wood");
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture;
            meshRenderer.material = material;
        }
        else if (PointHandler.setMaterialMetal)
        {
            Texture2D texture = Resources.Load<Texture2D>("metal");
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture;
            meshRenderer.material = material;
        }
        else
        {
            meshRenderer.material.color = currentColor;
        }

        return mesh;
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

    }
    public IEnumerator AnimateTriangles(Mesh mesh, List<Vector3> shape, List<Vector3> path, int speed)
    {
        int shapePointCount = shape.Count;
        int pathPointCount = path.Count;
        int triangleIndex = 0;
        int[] triangles = new int[(shapePointCount - 1) * (pathPointCount - 1) * 6];

        for (int i = 0; i < pathPointCount - 1; i++)
        {
            for (int j = 0; j < shapePointCount - 1; j++)
            {
                int current = i * shapePointCount + j;
                int next = (i + 1) * shapePointCount + j;

                // Define triangles in counter-clockwise order
                triangles[triangleIndex++] = current;
                triangles[triangleIndex++] = current + 1;
                triangles[triangleIndex++] = next;

                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = current + 1;
                triangles[triangleIndex++] = next + 1;

                if (triangleIndex % (speed * 6) == 0)
                {
                    mesh.triangles = triangles;
                    mesh.ManuallyRecalculateNormals();
                    // Wait for the next frame
                    yield return null;
                }
            }
        }

        // Ensure all triangles are set
        mesh.triangles = triangles;
        mesh.ManuallyRecalculateNormals();
    }

    public void StartAnimation(List<Vector3> shape, List<Vector3> path, int segments, Color currentColor, int speed)
    {
        Mesh mesh = CreateExtrudedMesh(shape, path, segments, currentColor);
        GetComponent<MeshFilter>().mesh = mesh;
        StartCoroutine(AnimateTriangles(mesh, shape, path, speed));
    }
    public void StartAnimationWithoutRotation(List<Vector3> shape, List<Vector3> path, int segments, Color currentColor, int speed)
    {
        Mesh mesh = CreateExtrudedMeshWithoutRotation(shape, path, segments, currentColor);
        GetComponent<MeshFilter>().mesh = mesh;
        StartCoroutine(AnimateTriangles(mesh, shape, path, speed));
    }
}
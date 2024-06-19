using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ExtrusionAxe : MonoBehaviour
{
    // Lista de puntos de la curva
    //public List<Vector3> curvePoints;
    public int segmentCount = 36;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ExtrudeSurAxe(List<Vector3> curve, Transform parent, Vector3 axis, Color currentColor)
    {
        if (curve == null || curve.Count < 2)
        {
            Debug.LogError("La lista de puntos de la curva está vacía o tiene menos de dos puntos.");
            return;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        transform.SetParent(parent);
        meshFilter.mesh = CreateRevolvedMesh(curve, segmentCount, axis, currentColor);
    }

    public Mesh CreateRevolvedMesh(List<Vector3> curve, int segments, Vector3 axisPosition, Color currentColor)
    {
        int curvePointCount = curve.Count;
        int verticesCount = curvePointCount * (segments + 1);

        Vector3[] vertices = new Vector3[verticesCount];
        int[] triangles = new int[segments * (curvePointCount - 1) * 6];
        Vector2[] uvs = new Vector2[verticesCount];

        // Create vertices
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Quaternion rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);

            for (int j = 0; j < curvePointCount; j++)
            {
                Vector3 point = curve[j];
                int vertexIndex = i * curvePointCount + j;

                // Translate point to the origin, rotate, then translate back
                Vector3 translatedPoint = point - axisPosition;
                Vector3 rotatedPoint = rotation * translatedPoint;
                vertices[vertexIndex] = rotatedPoint + axisPosition;

                uvs[vertexIndex] = new Vector2((float)i / segments, (float)j / (curvePointCount - 1));
            }
        }

        // Create triangles with correct winding order
        int triIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < curvePointCount - 1; j++)
            {
                int start = i * curvePointCount + j;
                int next = (i + 1) * curvePointCount + j;

                // Correct winding order
                triangles[triIndex++] = start;
                triangles[triIndex++] = start + 1;
                triangles[triIndex++] = next;

                triangles[triIndex++] = next;
                triangles[triIndex++] = start + 1;
                triangles[triIndex++] = next + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.ManuallyRecalculateNormals();

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

        // Opcional: Configurar más luces y parámetros de iluminación según sea necesario
    }
    public IEnumerator AnimateTriangles(Mesh mesh, List<Vector3> curve, int segments, int speed)
    {
        int curvePointCount = curve.Count;
        int triangleIndex = 0;
        int[] triangles = new int[segments * (curvePointCount - 1) * 6];

        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < curvePointCount - 1; j++)
            {
                int current = i * curvePointCount + j;
                int next = (i + 1) * curvePointCount + j;

                triangles[triangleIndex++] = current;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = current + 1;

                triangles[triangleIndex++] = current + 1;
                triangles[triangleIndex++] = next;
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
    }

    public void StartAnimation(List<Vector3> curve, int segments, Vector3 axis, Color currentColor, int speed)
    {
        Mesh mesh = CreateRevolvedMesh(curve, segments, axis, currentColor);
        GetComponent<MeshFilter>().mesh = mesh;
        StartCoroutine(AnimateTriangles(mesh, curve, segments, speed));
    }
}
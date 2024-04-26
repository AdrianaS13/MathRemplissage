using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Drawing : MonoBehaviour
{

    public List<Vector2Int> lastPolygonPixelVertices = new List<Vector2Int>();
    public List<List<Vector2Int>> polygons = new List<List<Vector2Int>>();

    public Camera mainCamera;
    public float maxZoom = 5f;
    public float minZoom = 1f;
    public float zoomSpeed = 1f;
    public Transform textureTransform;
    private bool isDragging = false;
    public bool isDrawingPixel = false;
    private Vector3 offset;

    public static Color Pen_Colour = Color.blue;

    public LayerMask Drawing_Layers;

    public bool Reset_Canvas_On_Play = true;
    public Color Reset_Colour = new Color(0, 0, 0, 0);

    public static Drawing drawable;
    Sprite drawable_sprite;
    public Texture2D drawable_texture;
    Color[] clean_colours_array;
    Color32[] cur_colors;

    bool is_drawing_line = false;
    Vector2 start_point;
    Vector2Int start_point1;
    Vector2 end_point;
    Vector2 first_point;
    public bool is_filling = false;
    public int x = 0;
    public int y = 0;
    public DrawingRelatedAlgo fillAlgoInstance;

    public List<List<Vector2Int>> pts = new List<List<Vector2Int>>();
    // Method to set pen brush color to red
    public void SetPenBrushRed()
    {
        Pen_Colour = red;
    }

    // Method to set pen brush color to blue
    public void SetPenBrushBlue()
    {
        Pen_Colour = blue;
    }

    // Method to set pen brush color to green
    public void SetPenBrushGreen()
    {
        Pen_Colour = green;
    }

    private Color red = Color.red;
    private Color blue = Color.blue;
    private Color green = Color.green;
    private Color white = Color.white;
    public Color[] penColors => new Color[] { red, green, blue, white };
    public int W => (int)drawable_sprite.rect.width;
    public int H => (int)drawable_sprite.rect.height;

    public void fill()
    {
        is_filling = true;
    }
    public void FillAll()
    {
        is_filling = false;
        foreach (var poly in polygons)
        {
            fillAlgoInstance.Fill(-1, -1, poly);
        }
    }
    public void paintInPixels(List<Vector3> points)
    {
        List<Vector2Int> pts1 = new List<Vector2Int>();
        // Iteramos sobre cada par de puntos consecutivos
        for (int i = 0; i < points.Count; i++)
        {
            float x = points[i].x;
            float y = points[i].y;
            Vector2Int pixel = WorldToPixelCoordinates(new Vector2(x, y));
            pts1.Add(pixel);

        }
        pts.Add(pts1);
    }
    public void paintPolygonsInPixels(List<Vector3> points)
    {
        // Iteramos sobre cada par de puntos consecutivos
        //for (int i = 0; i < points.Count; i++)
        //{
        //    float x = points[i].x;
        //    float y = points[i].y;
        //    Vector2Int pixel = WorldToPixelCoordinates(new Vector2(x, y));
        //    lastPolygonPixelVertices.Add(pixel);

        //}
        //polygons.Add(lastPolygonPixelVertices);
    }
    void DrawCurve(Vector2 start, Vector2 end)
    {
        cur_colors = drawable_texture.GetPixels32();
        Vector2Int start_pixel = WorldToPixelCoordinates(start);        
        Vector2Int end_pixel = WorldToPixelCoordinates(end);
        
        DrawLineSimple(start_pixel, end_pixel);
        ApplyMarkedPixelChanges();

        //start_point = end;
        is_drawing_line = true;
    }
    public static Vector2Int? PointInsidePoly(List<Vector2Int> v, int maxTry = 100)
    {
        Vector2Int low = new(v.Min(c => c.x), v.Min(c => c.y));
        Vector2Int high = new(v.Max(c => c.x), v.Max(c => c.y));
        // [-2,2]x[-2,2] around every point on poly, if not working we try random points inside the bounding box until max tries
        try
        {
            return Enumerable.Range(-2, 4).SelectMany(i => Enumerable.Range(-2, 4).Select(j => new Vector2Int(i, j)))
                                  .SelectMany(ij => v.Select(p => p + ij))
                                  .Concat(Enumerable.Range(0, maxTry).Select(cnt => new Vector2Int(Random.Range(low.x, high.x), Random.Range(low.x, high.x))))
                                  .First(finl => IsInsidePolygon(v, finl.x, finl.y));
        }
        //If no elements
        catch (InvalidOperationException e)
        {
            return null;
        }
    }
    // Method to set pen brush color to black
    public void SetPenBrushWhite()
    {
        Pen_Colour = white;
    }

    // Function to mark a pixel for color change
    public void MarkPixelToChange(int x, int y, Color color)
    {
        if (!TryGetArrayPos(x, y, out int array_pos))
            return;
        cur_colors[array_pos] = color;
    }

    private bool TryGetArrayPos(int x, int y, out int p)
    {
        if (y < 0 || y >= H || x < 0 || x >= W)
        {
            p = -1;
            return false;
        }
        p = y * (int)drawable_sprite.rect.width + x;
        return true;
    }
    public Color32? GetCurColor(int x, int y)
    {
        if (!TryGetArrayPos(x, y, out int array_pos))
            return null;
        return cur_colors[array_pos];
    }
    public bool TryGetCurColor(int x, int y, out Color32 color)
    {
        var c = GetCurColor(x, y);
        color = c.GetValueOrDefault();
        if (!c.HasValue)
            return false;
        return true;
    }

    // Function to draw a line between two points
    public void DrawLine(Vector2 start, Vector2 end)
    {
        cur_colors = drawable_texture.GetPixels32();
        Vector2Int start_pixel = WorldToPixelCoordinates(start);
        if (!is_drawing_line)
        {
            lastPolygonPixelVertices.Add(start_pixel);
        }
        Vector2Int end_pixel = WorldToPixelCoordinates(end);

        //DrawLineSimple(start_pixel, end_pixel);

        // Ajoute toujours le point final
        lastPolygonPixelVertices.Add(end_pixel);
        //ApplyMarkedPixelChanges();

        // Set the last point as the first point for the next line
        start_point = end;
        is_drawing_line = true;
    }
    public void drawEverything()
    {
        ResetCanvas();
        isDrawingPixel = true;
        
        /*for (int i = 0; i < polygons.Count; i++)
        {
            Debug.Log("polygons.Count" + polygons.Count);
            for (int j = 0; j < polygons[i].Count-1; j++)
            {
                Vector2Int point1 = polygons[i][j];
                Vector2Int point2 = polygons[i][j+1];
                //end_point = point2;
                DrawLineSimple(point1, point2);
                //ApplyMarkedPixelChanges();
            }
            //DrawLineSimple(polygons[i][0], end_point);
            //ApplyMarkedPixelChanges();
        }*/

        for (int i = 0; i < pts.Count; i++)
        {
            Debug.Log("pts.Count " + pts.Count);
            for (int j = 0; j < pts[i].Count - 1; j++)
            {
                Vector2Int point1 = pts[i][j];
                Vector2Int point2 = pts[i][j + 1];
                start_point1 = point2;
                DrawLineSimple(point1, point2);
                //ApplyMarkedPixelChanges();
            }
            DrawLineSimple(pts[i][0], start_point1);
            //ApplyMarkedPixelChanges();
        }
    }
    public void DrawLineSimple(Vector2Int start_pixel, Vector2Int end_pixel)
    {
        Vector2Int delta = end_pixel - start_pixel;

        if (delta == Vector2Int.zero)
            return;

        int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

        for (int i = 0; i <= steps; i++)
        {
            Vector2Int pixel = start_pixel + (delta * i / steps);
            MarkPixelToChange(pixel.x, pixel.y, Pen_Colour);
        }
        ApplyMarkedPixelChanges();
    }

    // Function to apply marked pixel changes to the texture
    public void ApplyMarkedPixelChanges()
    {
        drawable_texture.SetPixels32(cur_colors);
        drawable_texture.Apply();
    }

    // Function to convert world coordinates to pixel coordinates
    Vector2Int WorldToPixelCoordinates(Vector2 world_position)
    {
        Vector3 local_pos = transform.InverseTransformPoint(world_position);
        float pixelWidth = W;
        float pixelHeight = H;
        float unitsToPixels = pixelWidth / drawable_sprite.bounds.size.x * transform.localScale.x;
        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;
        return new Vector2Int(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));
    }

    // Function to check if the mouse pointer is over a UI object
    bool IsPointerOverUIObject()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // Function to get the mouse position in world coordinates
    Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void Update()
    {
        float zoomValue = Input.GetAxis("Mouse ScrollWheel");

        Vector3 zoomPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float newSize = mainCamera.orthographicSize - zoomValue * zoomSpeed;

        newSize = Mathf.Clamp(newSize, minZoom, maxZoom);

        float newSizeChange = newSize - mainCamera.orthographicSize;
        mainCamera.orthographicSize = newSize;

        mainCamera.transform.position += (zoomPoint - mainCamera.transform.position) * newSizeChange / mainCamera.orthographicSize;
        x = Convert.ToInt32(WorldToPixelCoordinates(GetMouseWorldPosition()).x);
        y = Convert.ToInt32(WorldToPixelCoordinates(GetMouseWorldPosition()).y);
        //var dbg = InsidePolygon(polygons, x, y);
        //Debug.Log($"{dbg == null} is {dbg}");
        if (Input.GetMouseButtonDown(2))
        {
            isDragging = true;

            Vector3 clickPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            offset = textureTransform.position - clickPosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(2))
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            textureTransform.position = mouseWorldPosition + offset;
        }
        if (Input.GetMouseButtonDown(0))
        {

            if (is_filling)
            {
                x = Convert.ToInt32(WorldToPixelCoordinates(GetMouseWorldPosition()).x);
                y = Convert.ToInt32(WorldToPixelCoordinates(GetMouseWorldPosition()).y);
                Debug.Log("x: " + x + ", " + y);
                fillAlgoInstance.Fill(x, y, InsidePolygon(polygons, x, y));
                is_filling = false;
            }
            else
            {
                Collider2D hit = Physics2D.OverlapPoint(GetMouseWorldPosition(), Drawing_Layers.value);
                if (hit != null && hit.transform != null || !IsPointerOverUIObject())
                {
                    if (!is_drawing_line)
                    {
                        first_point = GetMouseWorldPosition();
                        start_point = first_point;
                        is_drawing_line = true;
                    }
                    else
                    {
                        end_point = GetMouseWorldPosition();
                        DrawLine(start_point, end_point);
                    }
                }
            }

        }

        if (Input.GetMouseButtonDown(1))
        {
            if (is_drawing_line)
            {
                end_point = first_point;
                DrawLine(start_point, end_point);
                is_drawing_line = false;
                if (lastPolygonPixelVertices.Count >= 3)
                    polygons.Add(new List<Vector2Int>(lastPolygonPixelVertices));
                lastPolygonPixelVertices.Clear();
            }
        }
    }
    //public abstract void FillPolygon(List<Vector2Int> polygonToFill);
    protected static List<Vector2Int> InsidePolygon(List<List<Vector2Int>> polys, int x, int y)
    {
        return polys.FirstOrDefault((l) => IsInsidePolygon(l, x, y));
    }
    private static bool IsInsidePolygon(List<Vector2Int> polygon, int xI, int yI)
    {
        Vector2 point = new Vector2(xI, yI);
        float angleSum = 0;
        int n = polygon.Count;

        for (int i = 0; i < n; i++)
        {
            Vector2 v1 = polygon[i] - point;
            Vector2 v2 = polygon[(i + 1) % n] - point;

            /*float dot = Vector2.Dot(v1, v2);
            float magV1 = v1.magnitude;
            float magV2 = v2.magnitude;
            float cosTheta = dot / (magV1 * magV2);

            // Avoid division by zero in case of coincident points
            if (magV1 == 0 || magV2 == 0)
                return false;*/

            float angle = Vector2.SignedAngle(v1, v2);
            Debug.Log("Angle i : " + angle + ", ind " + i);
            angleSum += angle;
        }

        // Convert radians to degrees and check if sum is approximately 360
        //float angleSumDegrees = angleSum * (180f / (float)Mathf.PI);
        Debug.LogWarning("Sum : " + angleSum);
        // if (angleSum > .1f)
        return Mathf.Abs(Mathf.Abs(angleSum) - 360) < 0.1f;
    }
    IEnumerator operateStart()
    {

        yield return new WaitForSeconds(1);

    }
    // Function to reset the canvas
    public void ResetCanvas()
    {
        drawable_texture.SetPixels(clean_colours_array);
        drawable_texture.Apply();
    }
    public void ClearCanvas()
    {
        lastPolygonPixelVertices.Clear();
        polygons.Clear();
        pts.Clear();
        drawable_texture.SetPixels(clean_colours_array);
        drawable_texture.Apply();
    }
    void Awake()
    {
        drawable = this;
        drawable_sprite = this.GetComponent<SpriteRenderer>().sprite;
        drawable_texture = drawable_sprite.texture;
        cur_colors = drawable_texture.GetPixels32();

        clean_colours_array = new Color[W * H];
        for (int x = 0; x < clean_colours_array.Length; x++)
            clean_colours_array[x] = Reset_Colour;

        if (Reset_Canvas_On_Play)
            ResetCanvas();
    }


}

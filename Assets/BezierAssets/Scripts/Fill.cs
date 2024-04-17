using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Fill : MonoBehaviour
{
    public int width = 100; // Ancho de la textura
    public int height = 100; // Altura de la textura
    public Color fillColor = Color.white; // Color inicial de la textura
    public float lineWidth = 0.1f;
    public SpriteRenderer spriteRenderer; // Referencia al componente SpriteRenderer
    public Texture2D texture;
    public GameObject menuCanvas;
    void Start()
    {
        // Crea una nueva textura con el tamaño especificado
        texture = new Texture2D(width, height);

        // Rellena la textura con el color especificado
        Color[] fillColorArray = new Color[width * height];
        for (int i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = fillColor;
        }
        texture.SetPixels(fillColorArray);

        // Aplica los cambios a la textura
        texture.Apply();

        // Crea un nuevo Sprite utilizando la textura
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        // Asigna el Sprite al componente SpriteRenderer
        spriteRenderer.sprite = sprite;

        // Opcional: ajusta el tamaño del objeto SpriteRenderer para que coincida con el tamaño de la textura
        transform.localScale = new Vector3(texture.width, texture.height, 1);
    }
    public void ShowMenu()
    {
        menuCanvas.SetActive(true);
    }
    public void paintInPixels(List<Vector3> points)
    {
        // Iteramos sobre cada par de puntos consecutivos
        for (int i = 0; i < points.Count - 1; i++)
        {
            // Obtenemos los puntos inicial y final de la línea
            Vector3 startPoint = points[i];
            Vector3 endPoint = points[i + 1];
            DrawLinesPixels(startPoint, endPoint);

        }
        Vector3 firstPoint = points[0];
        Vector3 lastPoint = points[points.Count - 1];
        DrawLinesPixels(firstPoint, lastPoint);

    }
    public void DrawLinesPixels(Vector3 startPoint, Vector3 endPoint)
    {
        // Calculamos la distancia entre los puntos para determinar cuántos píxeles necesitamos interpolar
        float distance = Vector3.Distance(startPoint, endPoint);

        // Iteramos sobre los píxeles interpolar entre los puntos
        for (float t = 0; t < distance; t++)
        {
            // Interpolamos entre los puntos usando t
            Vector3 interpolatedPoint = Vector3.Lerp(startPoint, endPoint, t / distance);

            // Convertimos la posición interpolada a coordenadas locales del SpriteRenderer
            Vector3 localInterpolatedPoint = spriteRenderer.transform.InverseTransformPoint(interpolatedPoint);

            // Obtenemos las dimensiones del SpriteRenderer
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

            // Calculamos las coordenadas UV
            float uvX = (localInterpolatedPoint.x / spriteSize.x) + 0.5f;
            float uvY = (localInterpolatedPoint.y / spriteSize.y) + 0.5f;

            // Convertimos las coordenadas UV a coordenadas de píxeles en la textura
            int pixelX = Mathf.FloorToInt(uvX * spriteRenderer.sprite.texture.width);
            int pixelY = Mathf.FloorToInt(uvY * spriteRenderer.sprite.texture.height);

            // Pintamos el píxel en la textura
            Texture2D texture = spriteRenderer.sprite.texture;
            texture.SetPixel(pixelX, pixelY, Color.red);
        }
        // Aplicamos los cambios a la textura
        spriteRenderer.sprite.texture.Apply();
    }

    
    
}



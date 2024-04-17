using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fill : MonoBehaviour
{
    public int width = 100; // Ancho de la textura
    public int height = 100; // Altura de la textura
    public Color fillColor = Color.white; // Color inicial de la textura
    public SpriteRenderer spriteRenderer; // Referencia al componente SpriteRenderer
    public Texture2D texture;
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

    public void paintInPixel(LineRenderer lineRenderer, int i)
    {

        // Obtén la posición del Line Renderer (suponiendo que es la posición del primer punto)
        Vector3 position = lineRenderer.GetPosition(i);
        Debug.Log("punto line " + i + " : " + position);
        // Convierte la posición del mundo a coordenadas locales del SpriteRenderer
        Vector3 localPosition = spriteRenderer.transform.InverseTransformPoint(position);

        Debug.Log("localPosition " + i + " : " + localPosition);

        //Texture2D texture = spriteRenderer.sprite.texture;
        //int x = (int)localPosition.x;
        //int y = (int)localPosition.y;
        //texture.SetPixel(x, y, Color.red);
        //texture.Apply();
        // Obtén las dimensiones del SpriteRenderer
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // Calcule las coordenadas UV en el rango [0, 1] dentro del SpriteRenderer
        float uvX = (localPosition.x / spriteSize.x) + 0.5f;
        float uvY = (localPosition.y / spriteSize.y) + 0.5f;

        // Obtén las dimensiones de la textura del Sprite
        Vector2 textureSize = new Vector2(spriteRenderer.sprite.texture.width, spriteRenderer.sprite.texture.height);

        // Calcule las coordenadas de píxel en la textura del Sprite
        int pixelX = Mathf.FloorToInt(uvX * textureSize.x);
        int pixelY = Mathf.FloorToInt(uvY * textureSize.y);

        // Pinta el píxel en la imagen (asumiendo que la textura es modificable)
        texture = spriteRenderer.sprite.texture;
        texture.SetPixel(pixelX, pixelY, Color.red);
        texture.Apply();
    }
}

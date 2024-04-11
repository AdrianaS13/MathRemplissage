using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casteljau : MonoBehaviour
{
    [SerializeField] private int k;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void deCasteljau(List<Vector2Int> originalPolygonPoints)
    {
        for (int t = 0; t < 1; t+=(1/k))
        {
            for (int j=0; j< originalPolygonPoints.Count; j++)
            {
                for (int i=0; i< originalPolygonPoints.Count-j; i++)
                {
                    originalPolygonPoints[j] = (1 - t) * originalPolygonPoints[i] + t * originalPolygonPoints[i+1];
                }
            }
        }
    }
}

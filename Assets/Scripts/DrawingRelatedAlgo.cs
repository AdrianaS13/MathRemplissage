using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class DrawingRelatedAlgo : MonoBehaviour
{
    [SerializeField] protected NewFill _drawer;
    [SerializeField] Button _button;
    protected static Color _additionnalDrawingColor = new Color32(113, 113, 113, 255);
    
    protected bool PixelColorOrOut(int x, int y, Color32[] color, out Color32 found)
    {
        if (!_drawer.TryGetCurColor(x, y, out found))
            return true;
        //We need this cause out cannot be used in lambda
        var f = found;
        return color.Any(c => c.Equals(f));
    }
    
    protected bool PixelColorOrOut(int x, int y, Color32 color, out Color32 found)
    {
        return PixelColorOrOut(x, y, new Color32[] { color }, out found);
    }
    private void Awake()
    {
        //_button.onClick.AddListener(Operate);
        _button.onClick.AddListener(() => _drawer.fillAlgoInstance = this);
        _button.onClick.AddListener(_drawer.fill);
    }

    public abstract void Fill(int x,int y,List<Vector2Int> polygon);
}
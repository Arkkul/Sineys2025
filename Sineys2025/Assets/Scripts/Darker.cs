using UnityEngine;
using UnityEngine.UI;

public class Darker : MonoBehaviour
{
    [SerializeField] RawImage _image;
    [SerializeField] float _alpha = 0;
    [SerializeField] float _darkStep = 0.25f;
    [SerializeField] float _lightStep1 = 0.1f;

    public float Alpha
    {
        get {
            return _alpha;
        }
        set {
            _alpha = value;
            if (_alpha > 1)
            {
                _alpha = 0.8f;
            }
           
        }
    }

    public void MakeDarker()
    {
        Alpha += _darkStep;
       _image.color = new Color(255, 255, 255, Alpha);
       Debug.Log(_darkStep);
    }

    public void MakeVeryMuchLighter()
    {
        Alpha -= _lightStep1;
        _image.color = new Color(255, 255, 255, Alpha);
    }
    
    public void MakeMuchLighter()
    {
        Alpha -= _lightStep1/2;
        _image.color = new Color(255, 255, 255, Alpha);
    }

    public void MakeLighter()
    {
        Alpha -= _lightStep1 / 4;
        _image.color = new Color(255, 255, 255, Alpha);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Darker : MonoBehaviour
{
    [SerializeField] RawImage _image;
    [SerializeField] float _alpha = 0;
    [SerializeField] float _darkStep = 0.35f;
    [SerializeField] float _lightStep1 = 0.07f;
    [SerializeField] private AudioSource _lightMusic;
    [SerializeField] private AudioSource _darkMusic;

    public float Alpha
    {
        get {
            return _alpha;
        }
        set {
            _alpha = value;
            if (_alpha > 1)
            {
                _alpha = 1f;
            }
        }
    }

    public void MakeDarker()
    {
        Alpha += _darkStep;
        _lightMusic.volume -= _darkStep;
        _darkMusic.volume += _darkStep;
       _image.color = new Color(255, 255, 255, Alpha);
       Debug.Log(_darkStep);
    }

    public void MakeVeryMuchLighter()
    {
        _lightMusic.volume += _darkStep;
        _darkMusic.volume -= _darkStep;
        Alpha -= _lightStep1;
        _image.color = new Color(255, 255, 255, Alpha);
    }
    
    public void MakeMuchLighter()
    {
        _lightMusic.volume += _darkStep;
        _darkMusic.volume -= _darkStep;
        Alpha -= _lightStep1/2;
        _image.color = new Color(255, 255, 255, Alpha);
    }

    public void MakeLighter()
    {
        _lightMusic.volume += _darkStep;
        _darkMusic.volume -= _darkStep;
        Alpha -= _lightStep1 / 4;
        _image.color = new Color(255, 255, 255, Alpha);
    }
}

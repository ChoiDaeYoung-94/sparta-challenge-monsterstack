using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    private static BackgroundScroller _instance;
    public static BackgroundScroller Instance { get => _instance; }

    public Transform[] Layers;

    private float[] _originalScrollSpeeds = { 1.5f, 1f };
    private float[] _currentScrollSpeeds;
    private float[] _scrollSpeedsModifiers;
    private float _repeatWidth = 37.9f;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        Init();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void Init()
    {
        _currentScrollSpeeds = new float[_originalScrollSpeeds.Length];
        _scrollSpeedsModifiers = new float[_originalScrollSpeeds.Length];

        for (int i = 0; i < _scrollSpeedsModifiers.Length; i++)
        {
            _currentScrollSpeeds[i] = _originalScrollSpeeds[i];
            _scrollSpeedsModifiers[i] = _originalScrollSpeeds[i] / 3f;
        }

        Managers.UpdateManager.OnUpdateEvent -= LayerScroll;
        Managers.UpdateManager.OnUpdateEvent += LayerScroll;
    }

    private void LayerScroll()
    {
        for (int i = 0; i < Layers.Length; i++)
        {
            Layers[i].position += Vector3.left * _currentScrollSpeeds[i] * Time.deltaTime;

            if (Layers[i].position.x <= -_repeatWidth)
            {
                Layers[i].position = new Vector3(0f, Layers[i].position.y, Layers[i].position.z);
            }
        }
    }

    public void IncreaseSpeed()
    {
        for (int i = 0; i < _currentScrollSpeeds.Length; i++)
        {
            _currentScrollSpeeds[i] += _scrollSpeedsModifiers[i];
            _currentScrollSpeeds[i] = Mathf.Min(_currentScrollSpeeds[i], _originalScrollSpeeds[i]);
        }
    }

    public void DecreaseSpeed()
    {
        for (int i = 0; i < _currentScrollSpeeds.Length; i++)
        {
            _currentScrollSpeeds[i] -= _scrollSpeedsModifiers[i];
            _currentScrollSpeeds[i] = Mathf.Max(_currentScrollSpeeds[i], 0f);
        }
    }
}

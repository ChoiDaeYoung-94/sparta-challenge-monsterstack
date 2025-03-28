using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance { get => _instance; }

    private GameManager _gameManager;
    public static GameManager GameManager { get => _instance._gameManager; }


    [SerializeField] private UpdateManager _updateManager;
    public static UpdateManager UpdateManager { get => _instance._updateManager; }

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnDestroy()
    {
        _instance = null;
    }
}
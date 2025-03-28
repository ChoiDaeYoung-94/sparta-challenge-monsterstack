using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance { get => _instance; }

    private GameManager _gameManager;
    public static GameManager GameManager { get => _instance._gameManager; }

    private PoolManager _poolManager = new PoolManager();
    public static PoolManager PoolManager { get => _instance._poolManager; }

    [SerializeField] private UpdateManager _updateManager;
    public static UpdateManager UpdateManager { get => _instance._updateManager; }

    public GameObject[] PoolGameObjects;
    public GameObject[] PoolUIs;

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);

        Init();
    }

    private void OnDestroy()
    {
        _gameManager.Clear();
        _poolManager.Clear();

        _instance = null;
    }

    private void Init()
    {
        _poolManager.Init();
    }
}
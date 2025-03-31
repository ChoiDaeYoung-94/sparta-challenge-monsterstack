using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    #region Pool Class
    public class Pool
    {
        public GameObject TargetPrefab { get; private set; }
        public Transform Root { get; set; }
        public bool IsGameObjectPool = false;

        private Stack<PoolObject> _poolStack = new Stack<PoolObject>();

        public void Init(GameObject prefab, int count)
        {
            TargetPrefab = prefab;

            GameObject rootObject = new GameObject(prefab.name);
            if (!IsGameObjectPool)
            {
                rootObject.AddComponent<Canvas>();
            }
            Root = rootObject.transform;

            for (int i = 0; i < count; i++)
            {
                PoolObject poolObj = CreatePoolObject();
                PushToPool(poolObj);
            }
        }

        private PoolObject CreatePoolObject()
        {
            GameObject newObj = Object.Instantiate(TargetPrefab);
            newObj.name = TargetPrefab.name;
            PoolObject poolObj = newObj.GetComponent<PoolObject>();
            return poolObj;
        }

        public void PushToPool(PoolObject poolObj)
        {
            poolObj.transform.SetParent(Root);
            poolObj.gameObject.SetActive(false);

            _poolStack.Push(poolObj);
        }

        public GameObject PopFromPool(Transform parent)
        {
            PoolObject poolObj = _poolStack.Count > 0 ? _poolStack.Pop() : CreatePoolObject();
            poolObj.gameObject.SetActive(true);

            if (parent == null)
            {
                GameObject activePoolObj = GameObject.Find("ActivePool");
                parent = activePoolObj != null ? activePoolObj.transform : null;
            }
            poolObj.transform.SetParent(parent, false);

            return poolObj.gameObject;
        }
    }
    #endregion

    public Dictionary<string, Pool> PoolDictionary = new Dictionary<string, Pool>();

    public Transform RootGameObjects;
    public Transform RootUI;
    public Transform RootPlayer;

    public void Init()
    {
        RootGameObjects = new GameObject("Pool_GO").transform;
        Object.DontDestroyOnLoad(RootGameObjects.gameObject);

        RootUI = new GameObject("Pool_UI").transform;
        Object.DontDestroyOnLoad(RootUI.gameObject);

        RootPlayer = new GameObject("Pool_Player").transform;
        Object.DontDestroyOnLoad(RootPlayer.gameObject);

        for (int i = 0; i < Managers.Instance.PoolGameObjects.Length; i++)
        {
            CreatePool(Managers.Instance.PoolGameObjects[i], isGameObjectPool: true, count: 60);
        }

        for (int i = 0; i < Managers.Instance.PoolUIs.Length; i++)
        {
            CreatePool(Managers.Instance.PoolUIs[i], isGameObjectPool: false, count: 100);
        }
    }

    public void CreatePool(GameObject prefab, bool isGameObjectPool = true, int count = 20)
    {
        if (prefab == null)
        {
            return;
        }

        Pool pool = new Pool
        {
            IsGameObjectPool = isGameObjectPool
        };
        pool.Init(prefab, count);

        Transform rootParent = isGameObjectPool ? RootGameObjects : RootUI;
        pool.Root.SetParent(rootParent);

        if (!PoolDictionary.ContainsKey(prefab.name))
        {
            PoolDictionary.Add(prefab.name, pool);
        }
    }

    public void PushToPool(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        PoolObject poolObj = go.GetComponent<PoolObject>();
        if (poolObj == null || !PoolDictionary.ContainsKey(go.name))
        {
            Object.Destroy(go);
            return;
        }

        PoolDictionary[go.name].PushToPool(poolObj);
    }

    public GameObject PopFromPool(string poolName, Transform parent = null)
    {
        if (!PoolDictionary.ContainsKey(poolName))
        {
            return null;
        }

        return PoolDictionary[poolName].PopFromPool(parent);
    }

    public void Clear()
    {
        if (RootGameObjects != null)
        {
            foreach (Transform child in RootGameObjects)
            {
                Object.Destroy(child.gameObject);
            }
        }

        PoolDictionary.Clear();
    }
}
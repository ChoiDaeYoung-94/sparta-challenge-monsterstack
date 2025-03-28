using UnityEngine;

public class MainSceneInit : MonoBehaviour
{
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        Managers.GameManager.Init();
    }
}

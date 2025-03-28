using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    protected virtual void Awake()
    {
        Initialize();
    }

    protected abstract void Initialize();
}

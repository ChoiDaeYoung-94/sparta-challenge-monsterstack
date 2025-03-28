using System;
using UniRx;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    public event Action OnUpdateEvent;

    private void Awake()
    {
        Observable.EveryUpdate()
            .Subscribe(_ => OnUpdateEvent?.Invoke())
            .AddTo(this);
    }
}
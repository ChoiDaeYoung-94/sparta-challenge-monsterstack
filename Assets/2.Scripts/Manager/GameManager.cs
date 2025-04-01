using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class GameManager
{
    private float _minSpawnTime = 1f;
    private float _maxSpawnTime = 3f;

    private CancellationTokenSource _spawnMonsterCTS;

    public void Init()
    {
        _spawnMonsterCTS = new CancellationTokenSource();
        SpawnMonster(_spawnMonsterCTS.Token).Forget();
    }

    public void Clear()
    {
        _spawnMonsterCTS?.Cancel();
        _spawnMonsterCTS?.Dispose();
    }

    private async UniTask SpawnMonster(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            float waitTime = UnityEngine.Random.Range(_minSpawnTime, _maxSpawnTime);
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: cancellationToken);

            MonsterManager.Instance.CreateMonster();
        }
    }
}

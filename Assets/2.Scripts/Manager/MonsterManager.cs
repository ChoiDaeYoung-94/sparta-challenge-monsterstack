using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    private static MonsterManager _instance;
    public static MonsterManager Instance { get => _instance; }

    private List<Monster>[] _monstersByLine = new List<Monster>[3];

    private double _updateInterval = 0.1d;

    private CancellationTokenSource _updateMonsterOrderingCTS;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _updateMonsterOrderingCTS = new CancellationTokenSource();
        UpdateMonsterOrdering(_updateMonsterOrderingCTS.Token).Forget();
    }

    private void OnDestroy()
    {
        _updateMonsterOrderingCTS?.Cancel();
        _updateMonsterOrderingCTS?.Dispose();

        _instance = null;
    }

    private async UniTask UpdateMonsterOrdering(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_updateInterval), cancellationToken: token);

            OrderMonsters();
        }
    }

    private void OrderMonsters()
    {
        for (int i = 0; i < _monstersByLine.Length; i++)
        {
            _monstersByLine[i].Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            for (int j = 0; j < _monstersByLine[i].Count; j++)
            {
                if (j == 0)
                {
                    _monstersByLine[i][j].SetMonsterAhead(null);
                }
                else
                {
                    _monstersByLine[i][j].SetMonsterAhead(_monstersByLine[i][j - 1]);
                }
            }
        }
    }

    public void RegisterMonster(Monster monster, int line)
    {
        // 조건 생각 후 등록하는 코드 작업 필요

        OrderMonsters();
    }

    public void UnregisterMonster(Monster monster)
    {
        // 조건 생각 후 제거하는 코드 작업 필요

        OrderMonsters();
    }
}

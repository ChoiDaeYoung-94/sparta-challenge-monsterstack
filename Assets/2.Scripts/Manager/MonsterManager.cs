using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    private static MonsterManager _instance;
    public static MonsterManager Instance => _instance;

    private const int LineCount = 3;
    private const int MaxFloor = 5;
    private List<Monster>[,] _monstersByLineAndFloor;

    [SerializeField] private Transform[] _linePosition;
    private int[] _lineLayer = { -5, -4, -3 };
    private const int _maxMonsterCountPerFloor = 10;

    private float _collisionThreshold = 0.5f;
    private float _firstTargetPositionX = -0.5f;

    private double _updateInterval = 0.1d;
    private double _updateJumpInterval = 0.5d;
    private CancellationTokenSource _updateMonsterOrderingCTS;

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
        _updateMonsterOrderingCTS?.Cancel();
        _updateMonsterOrderingCTS?.Dispose();
        _instance = null;
    }

    private void Init()
    {
        _monstersByLineAndFloor = new List<Monster>[LineCount, MaxFloor];
        for (int line = 0; line < LineCount; line++)
        {
            for (int floor = 0; floor < MaxFloor; floor++)
            {
                _monstersByLineAndFloor[line, floor] = new List<Monster>();
            }
        }

        _updateMonsterOrderingCTS = new CancellationTokenSource();
        UpdateMonsterOrdering(_updateMonsterOrderingCTS.Token).Forget();
    }

    private async UniTask UpdateMonsterOrdering(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_updateInterval), cancellationToken: token);
            OrderMonstersDropAndMove();
            await UniTask.Delay(TimeSpan.FromSeconds(_updateJumpInterval), cancellationToken: token);
            OrderMonstersJump();
        }
    }

    #region OrderMonsters Methods
    /// <summary>
    /// 각 라인, 층 순환
    /// Drop -> Move -> Jump
    /// </summary>
    private void OrderMonstersDropAndMove()
    {
        for (int line = 0; line < LineCount; line++)
        {
            for (int floor = 0; floor < MaxFloor; floor++)
            {
                List<Monster> list = _monstersByLineAndFloor[line, floor];
                if (list.Count == 0)
                {
                    continue;
                }

                for (int i = 0; i < list.Count; i++)
                {
                    if (floor != 0 && i == 0 && list[0].transform.position.x <= _firstTargetPositionX + 0.05f)
                    {
                        if (list[0].SetDrop(floor - 1))
                        {
                            DemoteMonster(list[0]);
                            return;
                        }
                    }

                    float targetPositionX = _firstTargetPositionX + (i * _collisionThreshold);
                    list[i].SetMove(targetPositionX);
                }
            }
        }
    }

    /// <summary>
    /// 각 라인, 층 순환
    /// Jump
    /// </summary>
    private void OrderMonstersJump()
    {
        for (int line = 0; line < LineCount; line++)
        {
            for (int floor = 0; floor < MaxFloor; floor++)
            {
                List<Monster> list = _monstersByLineAndFloor[line, floor];
                if (list.Count == 0)
                {
                    continue;
                }

                if (floor + 1 >= MaxFloor)
                {
                    continue;
                }

                if (list.Count > 1)
                {
                    if (_monstersByLineAndFloor[line, floor + 1].Count + 1 > list.Count - 1)
                    {
                        continue;
                    }

                    for (int i = list.Count - 1; i > 0; i--)
                    {
                        Vector3 lastPosition = list[i].transform.position;
                        Vector3 secondLastPosition = list[i - 1].transform.position;

                        if (Vector3.Distance(lastPosition, secondLastPosition) >= _collisionThreshold - 0.05f &&
                            Vector3.Distance(lastPosition, secondLastPosition) <= _collisionThreshold + 0.05f)
                        {
                            list[i].SetJump(list[i - 1].transform.position.x);
                            PromoteMonster(list[i]);
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }
    }
    #endregion

    public void RegisterMonster(GameObject monsterObj, int line)
    {
        Monster monster = monsterObj.GetComponent<Monster>();
        monster.ResetMonster();
        monster.SetSortingGroup(_lineLayer[line]);
        monster.Line = line;
        monster.Floor = 0;
        _monstersByLineAndFloor[line, 0].Add(monster);

        OrderMonstersDropAndMove();
    }

    public void UnregisterMonster(Monster monster)
    {
        int line = monster.Line;
        int floor = monster.Floor;
        _monstersByLineAndFloor[line, floor].Remove(monster);

        OrderMonstersDropAndMove();
    }

    private void PromoteMonster(Monster monster)
    {
        int line = monster.Line;
        int currentFloor = monster.Floor;

        _monstersByLineAndFloor[line, currentFloor].Remove(monster);
        monster.Floor = currentFloor + 1;
        _monstersByLineAndFloor[line, currentFloor + 1].Add(monster);

        OrderMonstersDropAndMove();
    }

    private void DemoteMonster(Monster monster)
    {
        int line = monster.Line;
        int currentFloor = monster.Floor;

        _monstersByLineAndFloor[line, currentFloor].Remove(monster);
        monster.Floor = currentFloor - 1;
        _monstersByLineAndFloor[line, currentFloor - 1].Insert(0, monster);

        OrderMonstersDropAndMove();
    }

    int test = 0;
    public void CreateMonster()
    {
        //int randomLine = UnityEngine.Random.Range(0, LineCount);
        //if (_monstersByLineAndFloor[randomLine, 0].Count >= _maxMonsterCountPerFloor)
        //{
        //    return;
        //}

        int randomLine = 1;
        if (test++ >= 5)
            return;

        GameObject monsterObj = Managers.PoolManager.PopFromPool("ZombieMelee", _linePosition[randomLine]);
        RegisterMonster(monsterObj, randomLine);
    }
}

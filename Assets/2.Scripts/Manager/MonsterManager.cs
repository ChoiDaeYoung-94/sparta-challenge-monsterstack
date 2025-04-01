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

    private double _orderMonstersMoveAndDropInterval = 0.1d;
    private double _orderMonstersJumpInterval = 0.5d;
    private CancellationTokenSource _orderMonstersMoveAndDropCTS;
    private CancellationTokenSource _orderMonstersJumpCTS;

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
        _orderMonstersMoveAndDropCTS?.Cancel();
        _orderMonstersMoveAndDropCTS?.Dispose();
        _orderMonstersJumpCTS?.Cancel();
        _orderMonstersJumpCTS?.Dispose();
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

        _orderMonstersMoveAndDropCTS = new CancellationTokenSource();
        UpdateMonsterOrderingMove(_orderMonstersMoveAndDropCTS.Token).Forget();
        _orderMonstersJumpCTS = new CancellationTokenSource();
        UpdateMonsterOrderingJump(_orderMonstersJumpCTS.Token).Forget();
    }

    private async UniTask UpdateMonsterOrderingMove(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_orderMonstersMoveAndDropInterval), cancellationToken: token);
            OrderMonstersMoveAndDrop();
        }
    }

    private async UniTask UpdateMonsterOrderingJump(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_orderMonstersJumpInterval), cancellationToken: token);
            OrderMonstersJump();
        }
    }

    #region UpdateMonsterOrdering Methods
    private void OrderMonstersMoveAndDrop()
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
                        int lowFloorCount = _monstersByLineAndFloor[line, floor - 1].Count;
                        if (lowFloorCount > 3 && UnityEngine.Random.value < 0.4f)
                        {
                            continue;
                        }

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

    private void OrderMonstersJump()
    {
        for (int line = 0; line < LineCount; line++)
        {
            for (int floor = 0; floor < MaxFloor; floor++)
            {
                List<Monster> list = _monstersByLineAndFloor[line, floor];
                if (list.Count == 0 || floor + 1 >= MaxFloor)
                {
                    break;
                }

                if (list.Count > 1)
                {
                    for (int i = list.Count - 1; i > 0; i--)
                    {
                        Vector3 lastPosition = list[i].transform.position;
                        Vector3 secondLastPosition = list[i - 1].transform.position;
                        float distance = Vector3.Distance(lastPosition, secondLastPosition);
                        if (distance >= _collisionThreshold - 0.05f &&
                            distance <= _collisionThreshold + 0.05f &&
                            !list[i - 1].isJumping()                &&
                            _monstersByLineAndFloor[line, floor + 1].Count + 1 <= i)
                        {
                            list[i].SetJump(secondLastPosition.x);
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

    #region Monster Management Methods
    public void RegisterMonster(GameObject monsterObj, int line)
    {
        Monster monster = monsterObj.GetComponent<Monster>();
        monster.ResetMonster();
        monster.SetSortingGroup(_lineLayer[line]);
        monster.Line = line;
        monster.Floor = 0;
        _monstersByLineAndFloor[line, 0].Add(monster);
    }

    public void UnregisterMonster(Monster monster)
    {
        int line = monster.Line;
        int floor = monster.Floor;
        _monstersByLineAndFloor[line, floor].Remove(monster);
    }

    private void PromoteMonster(Monster monster)
    {
        int line = monster.Line;
        int currentFloor = monster.Floor;

        _monstersByLineAndFloor[line, currentFloor].Remove(monster);
        monster.Floor = currentFloor + 1;
        _monstersByLineAndFloor[line, currentFloor + 1].Add(monster);
    }

    private void DemoteMonster(Monster monster)
    {
        int line = monster.Line;
        int currentFloor = monster.Floor;

        _monstersByLineAndFloor[line, currentFloor].Remove(monster);
        monster.Floor = currentFloor - 1;
        _monstersByLineAndFloor[line, currentFloor - 1].Insert(0, monster);
    }
    #endregion

    int test = 0;
    public void CreateMonster()
    {
        int randomLine = UnityEngine.Random.Range(0, LineCount);
        if (_monstersByLineAndFloor[randomLine, 0].Count >= _maxMonsterCountPerFloor)
        {
            return;
        }

        GameObject monsterObj = Managers.PoolManager.PopFromPool("ZombieMelee", _linePosition[randomLine]);
        RegisterMonster(monsterObj, randomLine);
    }
}

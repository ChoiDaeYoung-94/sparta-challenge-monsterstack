using UnityEngine;
using UnityEngine.Rendering;

public enum MonsterType
{
    Melee,
    Ranged
}

public class Monster : Creature
{
    public MonsterType MonsterType;
    [SerializeField] private SortingGroup _sortingLayer;

    private float _moveSpeed = 2f;              // 기본 이동 속도
    private float _jumpHeight = 1f;             // 점프 시 높이
    private float _jumpDistanceOffset = 0.5f;   // 점프 후 x 오프셋
    private float _jumpDuration = 0.5f;         // 점프 지속 시간

    private bool _isJumping = false;
    private Vector3 _jumpStartPos;
    private Vector3 _jumpTargetPos;
    private float _jumpTimer = 0f;

    private Monster _monsterAhead;
    private float _collisionThreshold = 0.5f;   // 앞 몬스터와의 최소 거리

    private float _targetPositionX = -0.5f;     // 타겟과 닿는 위치
    private bool _reachedTarget = false;

    protected override void Initialize()
    {
        switch (MonsterType)
        {
            case MonsterType.Melee:
                _moveSpeed = 1f;
                break;
            case MonsterType.Ranged:
                _moveSpeed = 1.5f;
                break;
        }
    }

    private void Update()
    {
        if (_reachedTarget)
        {
            return;
        }

        if (transform.position.x <= _targetPositionX)
        {
            _reachedTarget = true;

            return;
        }

        if (_isJumping)
        {
            _jumpTimer += Time.deltaTime;
            float t = _jumpTimer / _jumpDuration;
            Vector3 newPos = Vector3.Lerp(_jumpStartPos, _jumpTargetPos, t);
            newPos.y += Mathf.Sin(t * Mathf.PI) * _jumpHeight;
            transform.position = newPos;

            if (t >= 1f)
            {
                _isJumping = false;
                _jumpTimer = 0f;
            }
        }
        else
        {
            transform.position += Vector3.left * _moveSpeed * Time.deltaTime;

            if (_monsterAhead != null)
            {
                float distance = Vector3.Distance(transform.position, _monsterAhead.transform.position);
                if (distance < _collisionThreshold)
                {
                    StartJump();
                }
            }
        }
    }

    private void StartJump()
    {
        _isJumping = true;
        _jumpStartPos = transform.position;
        _jumpTargetPos = _monsterAhead.transform.position + new Vector3(_jumpDistanceOffset, 0f, 0f);
    }

    public void SetSortingGroup(int layer)
    {
        _sortingLayer.sortingOrder = layer;
    }

    public void SetMonsterAhead(Monster monster)
    {
        _monsterAhead = monster;
    }
}

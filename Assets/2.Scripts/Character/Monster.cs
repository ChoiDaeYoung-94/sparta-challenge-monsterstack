using UnityEngine;
using UnityEngine.Rendering;

public enum MonsterType
{
    Melee,
    Ranged
}

public enum MonsterState
{
    Idle,
    Move,
    Jump,
    Drop
}

public class Monster : Creature
{
    public MonsterType MonsterType;
    [SerializeField] private MonsterState _monsterState;

    [SerializeField] private SortingGroup _sortingLayer;

    private float _moveSpeed;
    private float _targetPositionX;

    private bool _isJumping = false;
    private float _jumpHeight = 1f;
    private float _jumpDuration = 0.5f;
    private float _jumpTimer = 0f;
    private float _startPositionX;
    private float _startPositionY;
    private float _finalPositionY;

    private bool _isDropping = false;
    public float _dropDuration = 0.5f;
    private float _dropTimer = 0f;

    public int Line;
    public int Floor;

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

        _monsterState = MonsterState.Idle;
    }

    private void Update()
    {
        MonsterAI();
    }

    private void MonsterAI()
    {
        switch (_monsterState)
        {
            case MonsterState.Idle:
                return;
            case MonsterState.Move:
                Move();
                return;
            case MonsterState.Jump:
                Jump();
                return;
            case MonsterState.Drop:
                Drop();
                return;
            default:
                break;
        }
    }

    public void SetMove(float targetPositionX)
    {
        if (_isJumping || _isDropping)
        {
            return;
        }    

        _targetPositionX = targetPositionX;
        _monsterState = MonsterState.Move;
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position,
            new Vector3(_targetPositionX, transform.position.y, transform.position.z), _moveSpeed * Time.deltaTime);
    }

    public void SetJump(float targetPositionX)
    {
        if (_monsterState == MonsterState.Jump)
        {
            return;
        }

        _isJumping = true;
        _jumpTimer = 0f;
        _startPositionX = transform.position.x;
        _startPositionY = transform.localPosition.y;
        _finalPositionY = _startPositionY + _jumpHeight;
        _targetPositionX = targetPositionX;
        _monsterState = MonsterState.Jump;
    }

    private void Jump()
    {
        if (!_isJumping)
        {
            return;
        }

        _jumpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_jumpTimer / _jumpDuration);
        float newX = Mathf.Lerp(_startPositionX, _targetPositionX, t);
        float newY = Mathf.Lerp(_startPositionY, _finalPositionY, t) + 0.3f * Mathf.Sin(Mathf.PI * t);

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);

        if (t >= 1f)
        {
            _isJumping = false;
            transform.position = new Vector3(_targetPositionX, transform.position.y, transform.position.z);
            transform.localPosition = new Vector3(transform.localPosition.x, _finalPositionY, transform.position.z);
        }
    }

    public bool SetDrop(float targetPositionY)
    {
        if (_monsterState == MonsterState.Drop || _isJumping)
        {
            return false;
        }

        _isDropping = true;
        _dropTimer = 0f;
        _startPositionY = transform.localPosition.y;
        _finalPositionY = targetPositionY;
        _monsterState = MonsterState.Drop;

        return true;
    }

    private void Drop()
    {
        if (!_isDropping)
        {
            return;
        }

        _dropTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_dropTimer / _dropDuration);
        float newY;

        if (t < 0.7f)
        {
            float tSlow = t / 0.7f;
            newY = Mathf.Lerp(_startPositionY, _finalPositionY + 0.2f, tSlow);
        }
        else
        {
            float tFast = (t - 0.7f) / 0.3f;
            newY = Mathf.Lerp(_finalPositionY + 0.2f, _finalPositionY, tFast);
        }

        Vector3 localPos = transform.localPosition;
        localPos.y = newY;
        transform.localPosition = localPos;

        if (t >= 1f)
        {
            _isDropping = false;
            localPos.y = _finalPositionY;
            transform.localPosition = localPos;
        }
    }

    public void ResetMonster()
    {
        Initialize();
        transform.localPosition = Vector3.zero;
    }

    public void SetSortingGroup(int layer)
    {
        _sortingLayer.sortingOrder = layer;
    }
}

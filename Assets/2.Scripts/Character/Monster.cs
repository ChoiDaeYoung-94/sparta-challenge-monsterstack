using UnityEngine;

public enum MonsterType
{
    Melee,          // 근접 공격 몬스터
    LongRangeMelee  // 원거리 공격 몬스터
}

public class Monster : Creature
{
    public MonsterType MonsterType;

    private float _moveSpeed;
    public Transform target;

    protected override void Initialize()
    {
        switch (MonsterType)
        {
            case MonsterType.Melee:
                _moveSpeed = 1f;
                break;
            case MonsterType.LongRangeMelee:
                _moveSpeed = 1.5f;
                break;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            MoveTowardsTarget();
        }
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.position.x, transform.position.y, transform.position.z), _moveSpeed * Time.deltaTime);
    }
}

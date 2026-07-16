using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    [Header("References")]
    [SerializeField] private EnemyLocomotion _locomotion;
    [SerializeField] private EnemyStriker _striker;
    [SerializeField] private EnemyTargeting _targeting;
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody2D _rigidbody;

    public bool HasGroundAhead => _locomotion.HasGroundAhead();
    public float AttackRange => _striker.AttackRange;
    public bool IsStrikerOnCooldown => _striker.IsOnCooldown;

    private void Awake()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetFloat(SpeedHash, Mathf.Abs(_rigidbody.velocity.x));
    }

    public void Patrol() =>
        _locomotion.Patrol();

    public void Chase(ITargetable target) =>
        _locomotion.Chase(target.Position);

    public void Stop() =>
        _locomotion.Stop();

    public void FaceTowards(ITargetable target) =>
        _locomotion.FaceTowards(target.Position);

    public void BeginAttack()
    {
        _striker.BeginWindup();

        if (_animator == null)
        {
            return;
        }

        _animator.SetTrigger(AttackTriggerHash);
    }

    public bool TickAttackWindup() =>
        _striker.TickWindup(
            transform.position,
            _locomotion.FacingVector,
            _targeting.PlayerLayer);

    public void CancelStrikerWindup()
    {
        if (_striker == null)
        {
            return;
        }

        _striker.CancelWindup();
    }
}

using UnityEngine;

[RequireComponent(typeof(SpriteFacing))]
public class EnemyStrike : MonoBehaviour
{
    private const float AttackCircleRadiusFraction = 0.6f;
    private const float InitialLastAttackTime = -999f;

    [Header("Attack")]
    [SerializeField] private float _attackRange = 1f;
    [SerializeField] private int _attackDamage = 1;
    [SerializeField] private float _attackCooldown = 1.2f;
    [SerializeField] private float _attackWindup = 0.25f;
    [SerializeField] private float _attackOriginHeight = 0.8f;
    [SerializeField] private LayerMask _targetLayer;

    private SpriteFacing _facing;
    private float _lastAttackTime = InitialLastAttackTime;
    private float _windupTimer;
    private bool _isWindingUp;

    public float AttackRange => _attackRange;
    public bool IsOnCooldown => Time.time - _lastAttackTime < _attackCooldown;

    private void Awake()
    {
        _facing = GetComponent<SpriteFacing>();
    }

    private void OnDisable()
    {
        CancelWindup();
    }

    public bool BeginWindup()
    {
        if (_isWindingUp)
        {
            return false;
        }

        _isWindingUp = true;
        _windupTimer = _attackWindup;
        return true;
    }

    public bool TickWindup()
    {
        if (_isWindingUp == false)
        {
            return false;
        }

        _windupTimer -= Time.fixedDeltaTime;

        if (_windupTimer > 0f)
        {
            return false;
        }

        _isWindingUp = false;
        _lastAttackTime = Time.time;

        Vector2 attackOrigin = (Vector2)transform.position + Vector2.up * _attackOriginHeight;
        Vector2 direction = _facing.FacingVector;

        RaycastHit2D hit = Physics2D.CircleCast(
            attackOrigin,
            _attackRange * AttackCircleRadiusFraction,
            direction,
            _attackRange,
            _targetLayer);

        if (hit.collider == null)
        {
            return true;
        }

        if (hit.collider.TryGetComponent(out ITargetable target) == false)
        {
            return true;
        }

        target.TakeDamage(_attackDamage, transform.position);

        return true;
    }

    public void CancelWindup()
    {
        _isWindingUp = false;
    }
}

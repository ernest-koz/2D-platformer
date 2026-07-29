using UnityEngine;

[RequireComponent(typeof(SpriteFacing))]

public class EnemyStrike : MonoBehaviour
{
    private const float AttackCircleRadiusFraction = 0.6f;

    [Header("Attack")]
    [SerializeField, Min(0.01f)] private float _attackRange = 1f;
    [SerializeField, Min(1)] private int _attackDamage = 1;
    [SerializeField, Min(0f)] private float _attackCooldown = 1.2f;
    [SerializeField, Min(0f)] private float _attackWindup = 0.25f;
    [SerializeField] private float _attackOriginHeight = 0.8f;
    [SerializeField] private LayerMask _targetLayer;

    private SpriteFacing _facing;
    private float _cooldownTimer;
    private float _windupTimer;
    private bool _isWindingUp;

    public float AttackRange => _attackRange;
    public bool IsOnCooldown => _cooldownTimer > 0f;

    private void Awake()
    {
        _facing = GetComponent<SpriteFacing>();
    }

    private void OnDisable()
    {
        CancelWindup();
    }

    public void TickCooldown(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        if (_cooldownTimer <= 0f)
        {
            return;
        }

        _cooldownTimer = Mathf.Max(_cooldownTimer - deltaTime, 0f);
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

    public bool TickWindup(float deltaTime)
    {
        if (_isWindingUp == false)
        {
            return false;
        }

        _windupTimer -= Mathf.Max(deltaTime, 0f);

        if (_windupTimer > 0f)
        {
            return false;
        }

        _isWindingUp = false;
        _cooldownTimer = _attackCooldown;

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

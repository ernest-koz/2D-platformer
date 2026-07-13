using UnityEngine;

public class EnemyStriker : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float _attackRange = 1.0f;
    [SerializeField] private int _attackDamage = 1;
    [SerializeField] private float _attackCooldown = 1.2f;
    [SerializeField] private float _attackWindup = 0.25f;
    [SerializeField] private float _attackOriginHeight = 0.8f;
    [SerializeField] private LayerMask _obstacleLayer;

    private const float AttackCircleRadiusFraction = 0.6f;

    private float _lastAttackTime = -999f;
    private float _windupTimer;
    private bool _isWindingUp;

    public float AttackRange => _attackRange;
    public bool IsOnCooldown => Time.time - _lastAttackTime < _attackCooldown;

    public void BeginWindup()
    {
        if (_isWindingUp)
        {
            return;
        }

        _isWindingUp = true;
        _windupTimer = _attackWindup;
    }

    public bool TickWindup(Vector2 origin, Vector2 direction, LayerMask playerLayer)
    {
        if (_isWindingUp == false)
        {
            return false;
        }

        _windupTimer -= Time.deltaTime;

        if (_windupTimer > 0f)
        {
            return false;
        }

        _isWindingUp = false;
        _lastAttackTime = Time.time;

        Vector2 attackOrigin = origin + Vector2.up * _attackOriginHeight;
        var hit = Physics2D.CircleCast(
            attackOrigin,
            _attackRange * AttackCircleRadiusFraction,
            direction,
            _attackRange,
            playerLayer | _obstacleLayer);

        if (hit.collider != null && hit.collider.TryGetComponent(out PlayerHealth health))
        {
            health.TakeDamage(_attackDamage, transform.position);
        }

        return true;
    }

    public void CancelWindup()
    {
        _isWindingUp = false;
    }
}

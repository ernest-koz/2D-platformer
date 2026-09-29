using UnityEngine;

[RequireComponent(typeof(SpriteFacing))]
public class EnemyStrike : MonoBehaviour
{
    private const float AttackCircleRadiusFraction = 0.6f;

    [Header("Attack")]
    [SerializeField, Min(0.01f)] private float _attackRange = 1f;
    [SerializeField, Min(1)] private int _attackDamage = 1;
    [SerializeField, Min(0f)] private float _attackCooldownDuration = 1.2f;
    [SerializeField, Min(0f)] private float _attackWindupDuration = 0.25f;
    [SerializeField, Min(0f)] private float _attackOriginHeight = 0.8f;
    [SerializeField] private LayerMask _targetLayer;

    private SpriteFacing _facing;
    private float _remainingCooldownTime;
    private float _remainingWindupTime;
    private bool _isWindingUp;

    public float AttackRange => _attackRange;
    public bool IsOnCooldown => _remainingCooldownTime > 0f;

    private void Awake()
    {
        _facing = GetComponent<SpriteFacing>();
    }

    private void OnValidate()
    {
        if (_targetLayer.value == 0)
        {
            Debug.LogError($"{nameof(EnemyStrike)} target layer not assigned on {gameObject.name}.", gameObject);
        }
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

        if (_remainingCooldownTime <= 0f)
        {
            return;
        }

        _remainingCooldownTime = Mathf.Max(_remainingCooldownTime - deltaTime, 0f);
    }

    public bool BeginWindup()
    {
        if (_isWindingUp)
        {
            return false;
        }

        _isWindingUp = true;
        _remainingWindupTime = _attackWindupDuration;
        return true;
    }

    public bool TickWindup(float deltaTime)
    {
        if (_isWindingUp == false)
        {
            return false;
        }

        _remainingWindupTime -= Mathf.Max(deltaTime, 0f);

        if (_remainingWindupTime > 0f)
        {
            return false;
        }

        _isWindingUp = false;
        _remainingCooldownTime = _attackCooldownDuration;

        Vector2 attackOrigin = (Vector2)transform.position + Vector2.up * _attackOriginHeight;
        Vector2 direction = _facing.Vector;

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

        if (hit.collider.gameObject == gameObject)
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
        _remainingWindupTime = 0f;
    }
}

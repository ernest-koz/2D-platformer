using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyDeath : MonoBehaviour
{
    [Header("Death")]
    [SerializeField] private float _deathDelay = 0.6f;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameSession _gameSession;

    private Collider2D _collider;
    private Rigidbody2D _rigidbody;
    private bool _isDead;

    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    public bool IsDead => _isDead;

    public event Action<EnemyDeath> Died;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_gameSession == null)
        {
            Debug.LogError($"EnemyDeath {name}: GameSession not assigned. Enemy kills will not be tracked.", gameObject);
        }
    }

    private void Start()
    {
        _gameSession?.RegisterEnemy();
    }

    public void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        _animator?.SetTrigger(DieTriggerHash);

        _collider.enabled = false;
        _rigidbody.velocity = Vector2.zero;

        Died?.Invoke(this);

        _gameSession?.RegisterEnemyKill();

        Destroy(gameObject, _deathDelay);
    }
}

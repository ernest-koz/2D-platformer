using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyDeath : MonoBehaviour
{
    public event Action<EnemyDeath> Died;

    [Header("Death")]
    [SerializeField] private float _deathDelay = 0.6f;

    [Header("Refs")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameSession _gameSession;

    private Collider2D _collider;
    private Rigidbody2D _rigidbody;
    private bool _isDead;

    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    public bool IsDead => _isDead;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    public void Die()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        if (_animator != null)
        {
            _animator.SetTrigger(DieTriggerHash);
        }

        if (_collider != null)
        {
            _collider.enabled = false;
        }

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector2.zero;
        }

        Died?.Invoke(this);

        if (_gameSession != null)
        {
            _gameSession.RegisterEnemyKill();
        }

        Destroy(gameObject, _deathDelay);
    }
}

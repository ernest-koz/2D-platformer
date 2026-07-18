using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealthAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private Health _health;

    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    private void Awake()
    {
        _health = GetComponent<Health>();

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        _health.Damaged += OnDamaged;
        _health.Died += OnDied;
    }

    private void OnDisable()
    {
        _health.Damaged -= OnDamaged;
        _health.Died -= OnDied;
    }

    private void OnDamaged(Vector2 source)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetTrigger(HurtTriggerHash);
    }

    private void OnDied()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetTrigger(DieTriggerHash);
    }
}

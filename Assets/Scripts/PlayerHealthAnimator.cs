using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerHealthAnimator : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Animator _animator;

    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    private void Awake()
    {
        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        _playerHealth.Damaged += OnDamaged;
        _playerHealth.Died += OnDied;
    }

    private void OnDisable()
    {
        _playerHealth.Damaged -= OnDamaged;
        _playerHealth.Died -= OnDied;
    }

    private void OnDamaged(Vector2 source) =>
        _animator?.SetTrigger(HurtTriggerHash);

    private void OnDied() =>
        _animator?.SetTrigger(DieTriggerHash);
}

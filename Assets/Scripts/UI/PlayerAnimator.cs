using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (_animator == null)
        {
            Debug.LogError($"{nameof(PlayerAnimator)} Animator not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }
    }

    public void SetMovement(float speed, bool isGrounded)
    {
        _animator.SetFloat(SpeedHash, speed);
        _animator.SetBool(IsGroundedHash, isGrounded);
    }

    public void PlayHurt()
    {
        _animator.SetTrigger(HurtTriggerHash);
    }

    public void PlayDeath()
    {
        _animator.SetTrigger(DieTriggerHash);
    }
}

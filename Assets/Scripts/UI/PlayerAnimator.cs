using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");

    [SerializeField] private Animator _animator;

    private bool _isMissingReferenceReported;

    private void Awake()
    {
        HasTarget();
    }

    public void SetMovement(float speed, bool isGrounded)
    {
        if (HasTarget() == false)
        {
            return;
        }

        _animator.SetFloat(SpeedHash, speed);
        _animator.SetBool(IsGroundedHash, isGrounded);
    }

    public void PlayHurt()
    {
        if (HasTarget() == false)
        {
            return;
        }

        _animator.SetTrigger(HurtTriggerHash);
    }

    public void PlayDeath()
    {
        if (HasTarget() == false)
        {
            return;
        }

        _animator.SetTrigger(DieTriggerHash);
    }

    private bool HasTarget()
    {
        if (_animator == null)
        {
            if (_isMissingReferenceReported == false)
            {
                Debug.LogError(
                    $"{nameof(PlayerAnimator)} Animator not assigned on {gameObject.name}.",
                    gameObject);
                _isMissingReferenceReported = true;
            }

            return false;
        }

        return true;
    }
}

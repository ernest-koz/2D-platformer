using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int DieHash = Animator.StringToHash("Die");

    [SerializeField] private Animator _animator;

    private void OnValidate()
    {
        if (_animator == null)
        {
            Debug.LogError($"{nameof(PlayerAnimator)} animator not assigned on {gameObject.name}.", gameObject);
        }
    }

    public void SetMovement(float speed, bool isGrounded)
    {
        _animator.SetFloat(SpeedHash, speed);
        _animator.SetBool(IsGroundedHash, isGrounded);
    }

    public void PlayHurt()
    {
        _animator.SetTrigger(HurtHash);
    }

    public void PlayDeath()
    {
        _animator.SetTrigger(DieHash);
    }
}

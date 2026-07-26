using UnityEngine;

public sealed class EnemyAnimator
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private readonly Animator _target;

    public EnemyAnimator(Animator target)
    {
        _target = target;
    }

    public void SetSpeed(float speed)
    {
        _target.SetFloat(SpeedHash, speed);
    }

    public void PlayAttack()
    {
        _target.SetTrigger(AttackHash);
    }

    public void PlayDeath()
    {
        _target.SetTrigger(DieHash);
    }
}

using UnityEngine;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(EnemyTargeting))]
[RequireComponent(typeof(SpriteFacing))]
[RequireComponent(typeof(GroundDetector))]
public class EnemyChase : MonoBehaviour
{
    private Mover _mover;
    private EnemyTargeting _targeting;
    private SpriteFacing _facing;
    private GroundDetector _ground;

    private void Awake()
    {
        _mover = GetComponent<Mover>();
        _targeting = GetComponent<EnemyTargeting>();
        _facing = GetComponent<SpriteFacing>();
        _ground = GetComponent<GroundDetector>();
    }

    public bool Tick(float chaseRange)
    {
        ITargetable target = _targeting.FindNearestTarget(chaseRange);

        if (target == null)
        {
            return false;
        }

        if (_ground.IsGrounded == false)
        {
            return false;
        }

        float direction = target.Position.x - transform.position.x;
        _facing.Face(direction);

        if (_ground.HasGroundAhead(_facing.FacingDirection) == false)
        {
            _mover.Stop();
            return false;
        }

        _mover.Move(_facing.FacingDirection);

        return true;
    }
}

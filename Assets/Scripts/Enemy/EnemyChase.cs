using UnityEngine;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(SpriteFacing))]
[RequireComponent(typeof(GroundDetector))]
public class EnemyChase : MonoBehaviour
{
    private Mover _mover;
    private SpriteFacing _facing;
    private GroundDetector _ground;

    private void Awake()
    {
        _mover = GetComponent<Mover>();
        _facing = GetComponent<SpriteFacing>();
        _ground = GetComponent<GroundDetector>();
    }

    public bool Tick(ITargetable target)
    {
        if (target == null || _ground.IsGrounded == false)
        {
            return false;
        }

        _facing.Face(target.Position.x - transform.position.x);

        if (_ground.HasGroundAhead(_facing.FacingDirection) == false)
        {
            _mover.Stop();
            return false;
        }

        _mover.Move(_facing.FacingDirection);
        return true;
    }
}

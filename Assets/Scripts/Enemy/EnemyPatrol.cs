using UnityEngine;

[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(PatrolRoute))]
[RequireComponent(typeof(SpriteFacing))]
[RequireComponent(typeof(GroundDetector))]
public class EnemyPatrol : MonoBehaviour
{
    private Mover _mover;
    private PatrolRoute _route;
    private SpriteFacing _facing;
    private GroundDetector _ground;

    private void Awake()
    {
        _mover = GetComponent<Mover>();
        _route = GetComponent<PatrolRoute>();
        _facing = GetComponent<SpriteFacing>();
        _ground = GetComponent<GroundDetector>();
    }

    public void Tick()
    {
        if (_ground.IsGrounded == false)
        {
            _mover.Stop();
            return;
        }

        if (_ground.HasGroundAhead(_facing.FacingDirection) == false)
        {
            _facing.Flip();
            _mover.Stop();
            return;
        }

        float patrolDirection = _route.GetDirectionToward(transform.position.x, _facing.FacingDirection);

        _mover.Move(patrolDirection);

        if (patrolDirection < 0f && _facing.FacingDirection > 0)
        {
            _facing.Flip();
        }
        else if (patrolDirection > 0f && _facing.FacingDirection < 0)
        {
            _facing.Flip();
        }
    }
}

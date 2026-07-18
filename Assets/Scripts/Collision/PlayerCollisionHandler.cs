using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCollisionHandler : MonoBehaviour
{
    private const float StompHeightThreshold = 0.4f;
    private const float FallSpeedThreshold = 0.5f;

    private Rigidbody2D _rigidbody;

    public event Action<Pickup> PickupCollected;
    public event Action LevelFinished;
    public event Action<Collision2D> EnemyContacted;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Pickup pickup))
        {
            PickupCollected?.Invoke(pickup);
            Destroy(pickup.gameObject);
            return;
        }

        if (other.TryGetComponent(out FinishTrigger _))
        {
            LevelFinished?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Health enemyHealth) == false)
        {
            return;
        }

        if (enemyHealth.IsAlive == false)
        {
            return;
        }

        bool isStomp =
            transform.position.y > collision.transform.position.y + StompHeightThreshold &&
            _rigidbody.velocity.y < -FallSpeedThreshold;

        if (isStomp == false)
        {
            EnemyContacted?.Invoke(collision);
        }
    }
}

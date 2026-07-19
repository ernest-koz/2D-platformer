using System;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public event Action<Collider2D> TriggerEntered;
    public event Action<Collision2D> CollisionEntered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TriggerEntered?.Invoke(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEntered?.Invoke(collision);
    }
}

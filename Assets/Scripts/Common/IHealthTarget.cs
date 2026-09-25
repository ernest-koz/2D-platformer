using UnityEngine;

public interface IHealthTarget
{
    int TakeDamage(int amount, Vector2 sourcePosition);
    bool IsAlive { get; }
}

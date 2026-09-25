using UnityEngine;

public interface ITargetable
{
    Vector3 Position { get; }
    bool IsTargetable { get; }
    int TakeDamage(int amount, Vector2 sourcePosition);
}

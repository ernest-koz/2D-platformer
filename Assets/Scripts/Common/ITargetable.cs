using UnityEngine;

public interface ITargetable
{
    Vector3 Position { get; }
    bool IsTargetable { get; }
    void TakeDamage(int amount, Vector2 sourcePosition);
}

using UnityEngine;

public interface IStompable
{
    bool IsAvailable { get; }
    void TakeStompDamage(int amount, Vector2 sourcePosition);
}

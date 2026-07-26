using UnityEngine;

public interface IStompable
{
    bool IsAvailable { get; }
    void Defeat(Vector2 sourcePosition);
}

using UnityEngine;

/// <summary>
/// Любой объект, который враг может выбрать целью для атаки.
/// Враг не знает конкретный тип (игрок, другой враг, разрушаемый объект) —
/// только то, что у цели есть позиция, она может быть атакована и получает урон.
/// </summary>
public interface ITargetable
{
    /// <summary>Текущая позиция цели в world space.</summary>
    Vector3 Position { get; }

    /// <summary>Доступна ли цель для атаки сейчас (например, жива).</summary>
    bool IsTargetable { get; }

    /// <summary>Нанести цели урон от источника sourcePosition.</summary>
    void TakeDamage(int amount, Vector2 sourcePosition);
}

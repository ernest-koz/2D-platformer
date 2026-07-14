using System;

public interface IPickup<T>
{
    event Action<T> Collected;
}

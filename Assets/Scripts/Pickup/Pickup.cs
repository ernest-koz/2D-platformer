using System;
using UnityEngine;

public enum PickupType
{
    Coin,
    Health
}

public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupType _type;
    [SerializeField] private int _amount = 1;

    public PickupType Type => _type;
    public int Amount => _amount;

    public event Action<Pickup> Collected;

    public void Collect()
    {
        Collected?.Invoke(this);
    }
}

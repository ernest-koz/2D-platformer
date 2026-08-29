using System;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupType _type;
    [SerializeField, Min(1)] private int _amount = 1;

    private bool _isCollected;

    public event Action<Pickup> Collected;

    public PickupType Type => _type;
    public int Amount => _amount;
    public bool IsCollected => _isCollected;

    public void Collect()
    {
        if (_isCollected)
        {
            return;
        }

        _isCollected = true;
        Collected?.Invoke(this);
    }
}

public enum PickupType
{
    Coin,
    Health
}

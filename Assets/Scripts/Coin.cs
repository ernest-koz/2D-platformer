using UnityEngine;

public class Coin : Pickup
{
    [SerializeField] private int _value = 1;

    public int Value => _value;
}

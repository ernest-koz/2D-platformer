using UnityEngine;

public class HealthPickup : Pickup
{
    [SerializeField] private int _healAmount = 1;

    public int HealAmount => _healAmount;
}

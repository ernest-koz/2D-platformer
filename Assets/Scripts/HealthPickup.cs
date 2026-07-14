using System;
using UnityEngine;

public class HealthPickup : MonoBehaviour, IPickup<HealthPickup>
{
    [SerializeField] private int _healAmount = 1;

    public event Action<HealthPickup> Collected;

    public void Collect(GameObject collector)
    {
        if (collector.TryGetComponent(out PlayerHealth playerHealth) == false || playerHealth.IsAlive == false)
        {
            return;
        }

        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
        {
            return;
        }

        playerHealth.Heal(_healAmount);
        Collected?.Invoke(this);
    }
}

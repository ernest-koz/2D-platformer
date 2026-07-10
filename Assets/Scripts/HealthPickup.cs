using System;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public event Action<HealthPickup> Collected;

    [SerializeField] private int _healAmount = 1;

    public void Collect(GameObject collector)
    {
        var playerHealth = collector.GetComponent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsAlive == false)
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

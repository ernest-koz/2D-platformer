using UnityEngine;

public class HealButton : HealthChangerButton
{
    [SerializeField, Min(1)] private int _amount = 10;

    protected override void Apply()
    {
        Health.ReceiveHealing(_amount);
    }
}

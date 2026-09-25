using UnityEngine;

public class DamageButton : HealthChangerButton
{
    [SerializeField, Min(1)] private int _amount = 10;

    protected override void Apply()
    {
        Health.TakeDamage(_amount, transform.position);
    }
}

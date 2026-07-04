using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour, ICollectible
{
    [SerializeField] private int healAmount = 1;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void Collect(GameObject collector)
    {
        var health = collector.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.IsAlive) return;
        if (health.CurrentHealth >= health.MaxHealth) return;

        health.Heal(healAmount);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Collect(other.gameObject);
    }
}

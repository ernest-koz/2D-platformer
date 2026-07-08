using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour, ICollectible
{
    [FormerlySerializedAs("healAmount")] [SerializeField] private int _healAmount = 1;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") == false) return;

        Collect(other.gameObject);
    }

    public void Collect(GameObject collector)
    {
        var health = collector.GetComponentInParent<PlayerHealth>();

        if (health == null || health.IsAlive == false) return;

        if (health.CurrentHealth >= health.MaxHealth) return;

        health.Heal(_healAmount);
        Destroy(gameObject);
    }
}

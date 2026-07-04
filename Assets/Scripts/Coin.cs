using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour, ICollectible
{
    [SerializeField] private int value = 1;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void Collect(GameObject collector)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoin(value);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Collect(other.gameObject);
    }
}

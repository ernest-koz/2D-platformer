using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour, ICollectible
{
    [FormerlySerializedAs("value")] [SerializeField] private int _value = 1;

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
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoin(_value);

        Destroy(gameObject);
    }
}

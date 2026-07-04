using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FinishTrigger : MonoBehaviour
{
    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null)
            GameManager.Instance.FinishLevel();
    }
}

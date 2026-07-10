using UnityEngine;

public class HealthPickupSpawner : MonoBehaviour
{
    [SerializeField] private HealthPickup _prefab;
    [SerializeField] private Vector3[] _spawnPoints;
    [SerializeField] private Vector3 _spawnScale = Vector3.one;

    private void Awake()
    {
        if (_prefab == null)
        {
            return;
        }

        foreach (var spawnPoint in _spawnPoints)
        {
            var healthPickup = Instantiate(_prefab, spawnPoint, Quaternion.identity);
            healthPickup.transform.SetParent(transform, true);
            healthPickup.transform.localScale = _spawnScale;
            healthPickup.Collected += OnHealthPickupCollected;
        }
    }

    private void OnHealthPickupCollected(HealthPickup healthPickup)
    {
        healthPickup.Collected -= OnHealthPickupCollected;
        Destroy(healthPickup.gameObject);
    }
}

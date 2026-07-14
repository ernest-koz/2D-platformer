using UnityEngine;

public abstract class PickupSpawner<T> : MonoBehaviour where T : MonoBehaviour, IPickup<T>
{
    [SerializeField] private T _prefab;
    [SerializeField] private Vector3[] _spawnPoints;
    [SerializeField] private Vector3 _spawnScale = Vector3.one;

    private void Awake()
    {
        if (_prefab == null || _spawnPoints == null)
        {
            if (_prefab == null)
            {
                Debug.LogError($"PickupSpawner: prefab not assigned on {gameObject.name}.", gameObject);
            }

            if (_spawnPoints == null)
            {
                Debug.LogError($"PickupSpawner: spawnPoints not assigned on {gameObject.name}.", gameObject);
            }

            return;
        }

        foreach (Vector3 spawnPoint in _spawnPoints)
        {
            T pickup = Instantiate(_prefab, spawnPoint, Quaternion.identity);
            pickup.transform.SetParent(transform, true);
            pickup.transform.localScale = _spawnScale;
            Configure(pickup);
            pickup.Collected += OnCollected;
        }
    }

    private void OnCollected(T pickup)
    {
        pickup.Collected -= OnCollected;
        Destroy(pickup.gameObject);
    }

    protected virtual void Configure(T pickup)
    {
    }
}

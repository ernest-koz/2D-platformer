using UnityEngine;

public abstract class PickupSpawner<T> : MonoBehaviour where T : Pickup
{
    [SerializeField] private T _prefab;
    [SerializeField] private Vector3[] _spawnPoints;
    [SerializeField] private Vector3 _spawnScale = Vector3.one;

    private void Awake()
    {
        if (_prefab == null)
        {
            Debug.LogError($"PickupSpawner: prefab not assigned on {gameObject.name}.", gameObject);
            return;
        }

        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Debug.LogError($"PickupSpawner: spawnPoints empty on {gameObject.name}.", gameObject);
            return;
        }

        foreach (Vector3 spawnPoint in _spawnPoints)
        {
            T pickup = Instantiate(_prefab, spawnPoint, Quaternion.identity);
            pickup.transform.SetParent(transform, true);
            pickup.transform.localScale = _spawnScale;
            pickup.PickedUp += OnPickedUp;
            Configure(pickup);
        }
    }

    protected virtual void OnPickedUp(Pickup pickup) =>
        Destroy(pickup.gameObject);

    protected virtual void Configure(T pickup)
    {
    }
}

using System;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] private Pickup _prefab;
    [SerializeField] private Vector3[] _spawnPoints;
    [SerializeField] private Vector3 _spawnScale = Vector3.one;

    public int TotalCount => _spawnPoints == null ? 0 : _spawnPoints.Length;

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
            Pickup pickup = Instantiate(_prefab, spawnPoint, Quaternion.identity);
            pickup.transform.SetParent(transform, true);
            pickup.transform.localScale = _spawnScale;
            pickup.Collected += OnPickupCollected;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out Pickup pickup))
            {
                pickup.Collected -= OnPickupCollected;
            }
        }
    }

    private void OnPickupCollected(Pickup pickup)
    {
        pickup.Collected -= OnPickupCollected;
        Destroy(pickup.gameObject);
    }
}

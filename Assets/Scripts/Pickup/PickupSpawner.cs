using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] private Pickup _prefab;
    [SerializeField] private Vector3[] _spawnPoints;
    [SerializeField] private Vector3 _spawnScale = Vector3.one;

    private readonly List<Pickup> _spawnedPickups = new List<Pickup>();

    public int TotalCount => _spawnPoints.Length;

    private void Start()
    {
        foreach (Vector3 spawnPoint in _spawnPoints)
        {
            Pickup pickup = Instantiate(_prefab, spawnPoint, Quaternion.identity);
            pickup.transform.SetParent(transform, true);
            pickup.transform.localScale = _spawnScale;
            pickup.Collected += OnPickupCollected;
            _spawnedPickups.Add(pickup);
        }
    }

    private void OnDestroy()
    {
        foreach (Pickup pickup in _spawnedPickups)
        {
            pickup.Collected -= OnPickupCollected;
        }
    }

    private void OnValidate()
    {
        if (_prefab == null)
        {
            Debug.LogError($"PickupSpawner prefab not assigned on {gameObject.name}.", gameObject);
        }

        if (_spawnPoints.Length == 0)
        {
            Debug.LogError($"PickupSpawner spawn points empty on {gameObject.name}.", gameObject);
        }
    }

    private void OnPickupCollected(Pickup pickup)
    {
        pickup.Collected -= OnPickupCollected;
        Destroy(pickup.gameObject);
    }
}

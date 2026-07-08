using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PrefabSpawner : MonoBehaviour
{
    [Tooltip("Prefab to instantiate at every position in spawnPoints.")]
    [FormerlySerializedAs("prefab")] [SerializeField] private GameObject _prefab;
    [Tooltip("World positions where instances should appear.")]
    [FormerlySerializedAs("spawnPoints")] [SerializeField] private Vector3[] _spawnPoints;
    [Tooltip("Local scale applied to each spawned instance.")]
    [FormerlySerializedAs("spawnScale")] [SerializeField] private Vector3 _spawnScale = Vector3.one;

    private void Awake()
    {
        SpawnAll();
    }

    private void SpawnAll()
    {
        if (_prefab == null) return;

        foreach (var p in _spawnPoints)
        {
            var go = Instantiate(_prefab, p, Quaternion.identity);
            go.transform.SetParent(transform, true);
            go.transform.localScale = _spawnScale;
        }
    }
}

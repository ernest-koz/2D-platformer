using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Tooltip("Prefab to instantiate at every position in spawnPoints.")]
    [SerializeField] private GameObject prefab;
    [Tooltip("World positions where instances should appear.")]
    [SerializeField] private Vector3[] spawnPoints;
    [Tooltip("Local scale applied to each spawned instance.")]
    [SerializeField] private Vector3 spawnScale = Vector3.one;

    private readonly List<GameObject> _instances = new List<GameObject>();

    private void Awake()
    {
        SpawnAll();
        PlayerHealth.OnPlayerRespawn += RespawnAll;
    }

    private void OnDestroy()
    {
        PlayerHealth.OnPlayerRespawn -= RespawnAll;
    }

    private void SpawnAll()
    {
        if (prefab == null) return;
        foreach (var p in spawnPoints)
        {
            var go = Instantiate(prefab, p, Quaternion.identity);
            go.transform.SetParent(transform, true);
            go.transform.localScale = spawnScale;
            _instances.Add(go);
        }
    }

    private void RespawnAll()
    {
        foreach (var go in _instances)
            if (go != null) Destroy(go);
        _instances.Clear();
        SpawnAll();
    }
}

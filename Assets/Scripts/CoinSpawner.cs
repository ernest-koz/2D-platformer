using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private Coin _prefab;
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
            var coin = Instantiate(_prefab, spawnPoint, Quaternion.identity);
            coin.transform.SetParent(transform, true);
            coin.transform.localScale = _spawnScale;
            coin.Collected += OnCoinCollected;
        }
    }

    private void OnCoinCollected(Coin coin)
    {
        coin.Collected -= OnCoinCollected;
        Destroy(coin.gameObject);
    }
}

using UnityEngine;

public class CoinSpawner : PickupSpawner<Coin>
{
    [SerializeField] private GameSession _gameSession;

    protected override void Configure(Coin coin)
    {
        if (_gameSession == null)
        {
            Debug.LogError($"CoinSpawner {name}: GameSession not assigned. Coin count tracking disabled.", gameObject);
            return;
        }

        _gameSession.RegisterCoin();
    }
}

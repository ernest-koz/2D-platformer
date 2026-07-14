using UnityEngine;

public class CoinSpawner : PickupSpawner<Coin>
{
    [SerializeField] private GameSession _gameSession;

    protected override void Configure(Coin coin) =>
        coin.Initialize(_gameSession);
}

using System;
using UnityEngine;

public class Coin : MonoBehaviour, IPickup<Coin>
{
    [SerializeField] private int _value = 1;
    [SerializeField] private GameSession _gameSession;

    public event Action<Coin> Collected;

    private void Start()
    {
        if (_gameSession == null)
        {
            Debug.LogError($"Coin {name}: GameSession not assigned. Assign in Inspector or use CoinSpawner.", gameObject);
            return;
        }

        _gameSession.RegisterCoin();
    }

    public void Initialize(GameSession gameSession)
    {
        _gameSession = gameSession;
    }

    public void Collect()
    {
        if (_gameSession == null)
        {
            Debug.LogError($"Coin {name}: GameSession not assigned on Collect(). Assign in Inspector or use CoinSpawner.", gameObject);
            Collected?.Invoke(this);
            return;
        }

        _gameSession.AddCoin(_value);
        Collected?.Invoke(this);
    }
}

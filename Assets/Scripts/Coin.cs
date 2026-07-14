using System;
using UnityEngine;

public class Coin : MonoBehaviour, IPickup<Coin>
{
    [SerializeField] private int _value = 1;
    [SerializeField] private GameSession _gameSession;

    public event Action<Coin> Collected;

    public void Initialize(GameSession gameSession)
    {
        _gameSession = gameSession;
    }

    public void Collect()
    {
        if (_gameSession != null)
        {
            _gameSession.AddCoin(_value);
        }

        Collected?.Invoke(this);
    }
}

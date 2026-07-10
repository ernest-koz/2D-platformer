using System;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public event Action<Coin> Collected;

    [SerializeField] private int _value = 1;
    [SerializeField] private GameSession _gameSession;

    public void Initialize(GameSession gameSession)
    {
        _gameSession = gameSession;
    }

    public void Collect(GameObject collector)
    {
        if (_gameSession != null)
        {
            _gameSession.AddCoin(_value);
        }

        Collected?.Invoke(this);
    }
}

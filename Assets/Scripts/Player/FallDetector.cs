using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class FallDetector : MonoBehaviour
{
    [SerializeField] private float _deathY = -20f;

    private Health _health;
    private bool _isDead;

    public event Action FellToDeath;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        if (transform.position.y < _deathY)
        {
            _isDead = true;
            FellToDeath?.Invoke();
        }
    }
}

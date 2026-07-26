using System;
using UnityEngine;

public class FallDetector : MonoBehaviour
{
    [SerializeField] private float _deathY = -20f;

    private bool _isDead;

    public event Action FellToDeath;

    public void Check()
    {
        if (_isDead)
        {
            return;
        }

        if (transform.position.y >= _deathY)
        {
            return;
        }

        _isDead = true;
        FellToDeath?.Invoke();
    }
}

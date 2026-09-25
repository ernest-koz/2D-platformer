using System;
using UnityEngine;

public class FallDetector : MonoBehaviour
{
    [SerializeField] private float _deathY = -20f;

    private bool _hasReportedDeath;

    public event Action FellToDeath;

    public void Check()
    {
        if (_hasReportedDeath)
        {
            return;
        }

        if (transform.position.y >= _deathY)
        {
            return;
        }

        _hasReportedDeath = true;
        FellToDeath?.Invoke();
    }
}

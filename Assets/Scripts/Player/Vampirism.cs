using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(VampirismDamager))]
public class Vampirism : MonoBehaviour
{
    [Header("Vampirism")]
    [SerializeField, Min(0.1f)] private float _duration = 6f;
    [SerializeField, Min(0.1f)] private float _cooldownDuration = 4f;
    [SerializeField] private VampirismView _view;

    private VampirismDamager _damager;
    private Coroutine _abilityProcess;
    private bool _isRunning;

    public event Action<int> Drained;

    public bool IsActive => _isRunning;

    private void Awake()
    {
        _damager = GetComponent<VampirismDamager>();
    }

    private void OnDisable()
    {
        Interrupt();
    }

    private void OnValidate()
    {
        if (_view == null)
        {
            Debug.LogError($"{nameof(Vampirism)} view not assigned on {gameObject.name}.", gameObject);
        }
    }

    public void Activate()
    {
        if (_isRunning)
        {
            return;
        }

        _abilityProcess = StartCoroutine(AbilityProcess());
        _isRunning = true;
    }

    public void Interrupt()
    {
        if (_isRunning == false)
        {
            return;
        }

        StopCoroutine(_abilityProcess);
        _abilityProcess = null;
        _isRunning = false;

        if (_view == null)
        {
            return;
        }

        _view.Stop();
    }

    private IEnumerator AbilityProcess()
    {
        float elapsedTime = 0f;
        _view.StartAbility();

        while (elapsedTime < _duration)
        {
            elapsedTime += Time.deltaTime;
            Drain();
            _view.Tick(elapsedTime, _duration);
            yield return null;
        }

        _view.StartCooldown();
        float elapsedCooldown = 0f;

        while (elapsedCooldown < _cooldownDuration)
        {
            elapsedCooldown += Time.deltaTime;
            _view.Tick(elapsedCooldown, _cooldownDuration);
            yield return null;
        }

        _view.Stop();
        _abilityProcess = null;
        _isRunning = false;
    }

    private void Drain()
    {
        int drained = _damager.Tick(transform.position, Time.deltaTime);

        if (drained > 0)
        {
            Drained?.Invoke(drained);
        }
    }
}

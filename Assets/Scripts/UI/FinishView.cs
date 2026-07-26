using TMPro;
using System;
using UnityEngine;

public class FinishView : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _coinText;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _enemyText;
    [SerializeField] private string _coinFormat = "Монет собрано: {0} из {1}";
    [SerializeField] private string _timeFormat = "Время: {0:F1} с";
    [SerializeField] private string _enemyFormat = "Повержено врагов: {0} из {1}";

    public void Show(SessionStats stats)
    {
        if (HasRequiredReferences() == false)
        {
            return;
        }

        SetText(_coinText, _coinFormat, stats.TotalCoinsCollected, stats.TotalCoinsInLevel);
        SetText(_timeText, _timeFormat, stats.PlayTime);
        SetText(_enemyText, _enemyFormat, stats.EnemiesDefeated, stats.TotalEnemiesInLevel);

        _panel.SetActive(true);
    }

    private bool HasRequiredReferences()
    {
        if (_panel == null)
        {
            return ReportMissingReferences();
        }

        if (_coinText == null)
        {
            return ReportMissingReferences();
        }

        if (_timeText == null)
        {
            return ReportMissingReferences();
        }

        if (_enemyText == null)
        {
            return ReportMissingReferences();
        }

        return true;
    }

    private bool ReportMissingReferences()
    {
        Debug.LogError($"{nameof(FinishView)} references not assigned on {gameObject.name}.", gameObject);
        return false;
    }

    private void SetText(TMP_Text text, string format, params object[] args)
    {
        try
        {
            text.text = string.Format(format, args);
        }
        catch (FormatException exception)
        {
            Debug.LogError($"Invalid finish format on {gameObject.name}: {exception.Message}", gameObject);
        }
    }
}

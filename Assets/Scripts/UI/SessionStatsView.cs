using TMPro;
using UnityEngine;

public class SessionStatsView : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _coinText;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _enemyText;
    [SerializeField] private string _coinFormat = "Монет собрано: {0} из {1}";
    [SerializeField] private string _timeFormat = "Время: {0:F1} с";
    [SerializeField] private string _enemyFormat = "Повержено врагов: {0} из {1}";

    private void OnValidate()
    {
        if (_panel == null)
        {
            Debug.LogError($"{nameof(SessionStatsView)} panel not assigned on {gameObject.name}.", gameObject);
        }

        if (_coinText == null)
        {
            Debug.LogError($"{nameof(SessionStatsView)} coin text not assigned on {gameObject.name}.", gameObject);
        }

        if (_timeText == null)
        {
            Debug.LogError($"{nameof(SessionStatsView)} time text not assigned on {gameObject.name}.", gameObject);
        }

        if (_enemyText == null)
        {
            Debug.LogError($"{nameof(SessionStatsView)} enemy text not assigned on {gameObject.name}.", gameObject);
        }

        if (TextFormatUtility.IsValid(_coinFormat, 0, 0) == false)
        {
            Debug.LogError($"{nameof(SessionStatsView)} coin format is invalid on {gameObject.name}.", gameObject);
        }

        if (TextFormatUtility.IsValid(_timeFormat, 0f) == false)
        {
            Debug.LogError($"{nameof(SessionStatsView)} time format is invalid on {gameObject.name}.", gameObject);
        }

        if (TextFormatUtility.IsValid(_enemyFormat, 0, 0) == false)
        {
            Debug.LogError($"{nameof(SessionStatsView)} enemy format is invalid on {gameObject.name}.", gameObject);
        }
    }

    public void Show(SessionStats stats)
    {
        _coinText.text = string.Format(_coinFormat, stats.TotalCoinsCollected, stats.TotalCoinsInLevel);
        _timeText.text = string.Format(_timeFormat, stats.PlayTime);
        _enemyText.text = string.Format(_enemyFormat, stats.EnemiesDefeated, stats.TotalEnemiesInLevel);

        _panel.SetActive(true);
    }
}

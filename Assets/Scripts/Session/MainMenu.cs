using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private const string GameSceneName = "SampleScene";

    [SerializeField] private GameObject _authorsPanel;

    private void Awake()
    {
        if (_authorsPanel == null)
        {
            Debug.LogError($"{nameof(MainMenu)} authors panel not assigned on {gameObject.name}.", gameObject);
            enabled = false;
        }
    }

    private void Start()
    {
        HideAuthors();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void ShowAuthors()
    {
        _authorsPanel.SetActive(true);
    }

    public void HideAuthors()
    {
        _authorsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

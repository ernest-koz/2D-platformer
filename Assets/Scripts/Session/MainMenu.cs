using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private const string GameSceneName = "SampleScene";

    [SerializeField] private GameObject _authorsPanel;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _authorsButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _backButton;

    private void OnEnable()
    {
        _playButton.onClick.AddListener(OnPlayButtonPressed);
        _authorsButton.onClick.AddListener(OnAuthorsButtonPressed);
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
        _backButton.onClick.AddListener(OnBackButtonPressed);
    }

    private void Start()
    {
        HideAuthors();
    }

    private void OnDisable()
    {
        _playButton.onClick.RemoveListener(OnPlayButtonPressed);
        _authorsButton.onClick.RemoveListener(OnAuthorsButtonPressed);
        _quitButton.onClick.RemoveListener(OnQuitButtonPressed);
        _backButton.onClick.RemoveListener(OnBackButtonPressed);
    }

    private void OnValidate()
    {
        if (_authorsPanel == null)
        {
            Debug.LogError($"{nameof(MainMenu)} authors panel not assigned on {gameObject.name}.", gameObject);
        }

        if (_playButton == null)
        {
            Debug.LogError($"{nameof(MainMenu)} play button not assigned on {gameObject.name}.", gameObject);
        }

        if (_authorsButton == null)
        {
            Debug.LogError($"{nameof(MainMenu)} authors button not assigned on {gameObject.name}.", gameObject);
        }

        if (_quitButton == null)
        {
            Debug.LogError($"{nameof(MainMenu)} quit button not assigned on {gameObject.name}.", gameObject);
        }

        if (_backButton == null)
        {
            Debug.LogError($"{nameof(MainMenu)} back button not assigned on {gameObject.name}.", gameObject);
        }
    }

    public void ShowAuthors()
    {
        _authorsPanel.SetActive(true);
    }

    public void HideAuthors()
    {
        _authorsPanel.SetActive(false);
    }

    private void OnPlayButtonPressed()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    private void OnAuthorsButtonPressed()
    {
        ShowAuthors();
    }

    private void OnQuitButtonPressed()
    {
        Application.Quit();
    }

    private void OnBackButtonPressed()
    {
        HideAuthors();
    }
}

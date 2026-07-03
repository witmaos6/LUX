using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartNewGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        if (continueButton != null)
            continueButton.interactable = SaveManager.HasSave;
    }

    private void StartNewGame()
    {
        SaveManager.ResetSave();
        SceneManager.LoadScene(mainSceneName);
    }

    private void ContinueGame()
    {
        if (!SaveManager.HasSave) return;
        SceneManager.LoadScene(mainSceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

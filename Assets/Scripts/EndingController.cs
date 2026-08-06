using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingController : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private Button homeButton;

    private void Awake()
    {
        if(homeButton)
        {
            homeButton.onClick.AddListener(ToLobby);
        }
    }

    public void ToLobby()
    {
        SceneManager.LoadScene(lobbySceneName);
    }
}

using UnityEngine;

public class ReturnToMainMenuAfterGameOver : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanelCOOP;
    [SerializeField] private GameObject lobbyPanelPVP;
    [SerializeField] private GameObject menuPanel;

    private void Start()
    {
        if (GameOverSceneManager.OpenLobbyAfterLoad)
        {
            if (!GameModeManager.Instance.IsPvPMode)
            {
                lobbyPanelCOOP.SetActive(true);
                menuPanel.SetActive(false);
                GameOverSceneManager.OpenLobbyAfterLoad = false; // reset flag
            }
            else
            {
                lobbyPanelPVP.SetActive(true);
                menuPanel.SetActive(false);
                GameOverSceneManager.OpenLobbyAfterLoad = false; // reset flag
            }

        }
    }

}

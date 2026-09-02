using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Multiplayer;

public class LobbyBox : MonoBehaviour
{
    [SerializeField] TMP_Text lobbyName;
    [SerializeField] TMP_Text players;
    [SerializeField] TMP_Text difficulty;
    [SerializeField] Button joinButton;

    private ISessionInfo session;


    public void Initialize(ISessionInfo newSession)
    {
        session = newSession;

        lobbyName.text = session.Name;

        int currentPlayers = session.MaxPlayers - session.AvailableSlots;
        players.text = $"{currentPlayers}/{session.MaxPlayers}";

        if (session.Properties != null &&
            session.Properties.TryGetValue("Difficulty", out var diff))
        {
            difficulty.text = diff.Value;
        }
        else
        {
            difficulty.text = "-";
        }

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(JoinLobby);
    }

    void JoinLobby()
    {
        joinButton.interactable = false;
        if (session == null) { Debug.LogWarning("Session is Null Can't join" + name); return; }
        LobbyUIManager.Instance.OnJoin(session.Id);

    }
}
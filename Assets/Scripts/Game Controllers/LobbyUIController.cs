using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

using NickType = StringContainer;


/// Must be placed on the same object as LobbyNetwork
public class LobbyUIController : NetworkBehaviour
{
    public static LobbyUIController Instance;

    [SerializeField] private Image _loadingWheel;
    [SerializeField] private float _loadingWheelRotationSpeed;
    [SerializeField] private int _maxCodeLength;

    private LobbyNetwork _lobbyNetwork;

    private State _currentState = State.NOT_CONNECTED;
    private NickType[] _teamANicks;
    private NickType[] _teamBNicks;
    private string _nick = "";
    private string _joinCode = "";

    public State CurrentState
    {
        set => _currentState = value;
    }

    public enum State : byte
    {
        NOT_CONNECTED,
        CONNECTING,
        NICK_CHOOSING,
        TEAM_CHOOSING
    }

    public void SetJoinCode(string joinCode)
    {
        _joinCode = joinCode;
    }

    public void NickTablesUpdate(NickType[] teamANicks, NickType[] teamBNicks)
    {
        _teamANicks = teamANicks;
        _teamBNicks = teamBNicks;
    }

    private void Awake()
    {
        Instance = this;
        _lobbyNetwork = GetComponent<LobbyNetwork>();
    }

    private void FixedUpdate()
    {
        DisplayLoadingWheel();
    }

    private void DisplayLoadingWheel()
    {
        _loadingWheel.enabled = (_currentState == State.CONNECTING);
        if (_currentState == State.CONNECTING)
            _loadingWheel.rectTransform.Rotate(Vector3.forward, _loadingWheelRotationSpeed);
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0f, 0f, 200f, 200));

        switch (_currentState)
        {
            case State.NOT_CONNECTED:
                NotConnectedUI();
                break;

            case State.NICK_CHOOSING:
                NickChoosingUI();
                break;

            case State.TEAM_CHOOSING:
                TeamChoosingUI();
                break;
        }

        GUILayout.EndArea();
    }


    private void NotConnectedUI()
    {
        _joinCode = GUILayout.TextField(_joinCode, _maxCodeLength);

        if (GUILayout.Button("Global host"))
        {
            _currentState = State.CONNECTING;
            NetworkController.Instance.CreateGlobalGame();
        }
        if (GUILayout.Button("Global client"))
        {
            _currentState = State.CONNECTING;
            NetworkController.Instance.JoinGlobalGame(_joinCode);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Local host"))
        {
            _currentState = State.CONNECTING;
            NetworkController.Instance.CreateLocalGame();
        }
        if (GUILayout.Button("Local client"))
        {
            _currentState = State.CONNECTING;
            NetworkController.Instance.JoinLocalGame();
        }
    }

    private void NickChoosingUI()
    {
        GUILayout.Label("Your nick:");
        _nick = GUILayout.TextField(_nick);

        if (GUILayout.Button("Submit") && _nick.Trim().Length != 0)
        {
            _lobbyNetwork.NickUpdateServerRpc(new(_nick));
            _currentState = State.TEAM_CHOOSING;
        }
    }

    private void TeamChoosingUI()
    {
        // First column
        if (GUILayout.Button("Team A") && !LobbyPlayerData.LocalPlayer.Ready)
        {
            _lobbyNetwork.TeamUpdateServerRpc(Team.A);
        }
        foreach (var nick in _teamANicks ?? Enumerable.Empty<NickType>())
            if (nick.Text.Trim().Length != 0) GUILayout.Label(nick.Text);
        GUILayout.EndArea();

        // Second column
        GUILayout.BeginArea(new Rect(200f, 0f, 200f, 200));
        if (GUILayout.Button("Team B") && !LobbyPlayerData.LocalPlayer.Ready)
        {
            _lobbyNetwork.TeamUpdateServerRpc(Team.B);
        }
        foreach (var nick in _teamBNicks ?? Enumerable.Empty<NickType>())
            if (nick.Text.Trim().Length != 0) GUILayout.Label(nick.Text);
        GUILayout.EndArea();

        // Ready button
        GUILayout.BeginArea(new Rect(0f, 200f, 200f, 200f));

        if (GUILayout.Button(LobbyPlayerData.LocalPlayer.Ready ? "Not Ready" : "Ready"))
            LobbyPlayerData.LocalPlayer.ToggleReady();

        // Start button
        if (IsServerAndAllPlayersAreReady() && GUILayout.Button("Start"))
            NetworkController.Instance.LoadGameScene();

        DisplayJoinCode();
    }

    private bool IsServerAndAllPlayersAreReady()
    {
        return IsServer && LobbyPlayerDataManager.Instance.AreAllPlayersReady();
    }

    private void DisplayJoinCode()
    {
        if (_joinCode != string.Empty)
            GUILayout.Label("Join code: " + _joinCode);
    }
}

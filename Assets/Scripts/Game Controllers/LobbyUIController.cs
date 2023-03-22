using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using NickType = StringContainer;

/// <summary>
/// Must be placed on the same object as LobbyNetwork
/// </summary>
public class LobbyUIController : MonoBehaviour
{
    public static LobbyUIController Instance;

    [SerializeField] private Image _loadingWheel;
    [SerializeField] private float _loadingWheelRotationSpeed;
    [SerializeField] private int _maxCodeLength;

    private LobbyNetwork _lobbyNetwork;

    private State _currentState = State.NOT_CONNECTED;
    private NickType[] _teamANicks;
    private NickType[] _teamBNicks;
    private bool _ready = false;
    private bool _startAvailable = false;
    private string _nick = "";
    private string _joinCode = "";


    public State CurrentState { get => _currentState; set => _currentState = value; }


    public bool StartAvailable { set => _startAvailable = value; }


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

    private void Awake()
    {
        Instance = this;
        _lobbyNetwork = GetComponent<LobbyNetwork>();
    }


    private void FixedUpdate()
    {
        var loadingWheelEnabled = false;

        // Displaying and rotating loading wheel
        if (_currentState == State.CONNECTING)
        {
            loadingWheelEnabled = true;
            _loadingWheel.rectTransform.Rotate(new Vector3(0f, 0f, _loadingWheelRotationSpeed));
        }

        _loadingWheel.enabled = loadingWheelEnabled;
    }


    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0f, 0f, 200f, 200));

        switch (_currentState)
        {
            case State.NOT_CONNECTED:
                NotConnected();
                break;

            case State.NICK_CHOOSING:
                NickChoosing();
                break;

            case State.TEAM_CHOOSING:
                TeamChoosing();
                break;
        }

        GUILayout.EndArea();
    }


    private void NotConnected()
    {
        _joinCode = GUILayout.TextField(_joinCode, _maxCodeLength);
        if (GUILayout.Button("Global host")) { _currentState = State.CONNECTING; NetworkController.Instance.CreateGlobalGame(); }
        if (GUILayout.Button("Global client")) { _currentState = State.CONNECTING; NetworkController.Instance.JoinGlobalGame(_joinCode); }

        GUILayout.Space(10);

        if (GUILayout.Button("Local host")) { _currentState = State.CONNECTING; NetworkController.Instance.CreateLocalGame(); }
        if (GUILayout.Button("Local client")) { _currentState = State.CONNECTING; NetworkController.Instance.JoinLocalGame(); }
    }


    private void NickChoosing()
    {
        GUILayout.Label("Your nick:");
        _nick = GUILayout.TextField(_nick);
        if (GUILayout.Button("Submit") && _nick.Trim().Length != 0)
        {
            _lobbyNetwork.NickUpdateServerRpc(new(_nick));
            _lobbyNetwork.ReadyUpdateServerRpc(ready: false);
            //_lobbyNetwork.ReadyUpdateServerRpc(_ready);
            _currentState = State.TEAM_CHOOSING;
        }
    }


    public void NickTablesUpdate(NickType[] teamANicks, NickType[] teamBNicks)
    {
        _teamANicks = teamANicks;
        _teamBNicks = teamBNicks;
    }


    private void TeamChoosing()
    {
        // First column
        if (GUILayout.Button("Team A") && !_ready)
        {
            _lobbyNetwork.TeamUpdateServerRpc(Team.A);
        }
        foreach (var nick in _teamANicks ?? Enumerable.Empty<NickType>())
            if (nick.Text.Trim().Length != 0) GUILayout.Label(nick.Text);
        GUILayout.EndArea();

        // Second column
        GUILayout.BeginArea(new Rect(200f, 0f, 200f, 200));
        if (GUILayout.Button("Team B") && !_ready)
        {
            _lobbyNetwork.TeamUpdateServerRpc(Team.B);
        }
        foreach (var nick in _teamBNicks ?? Enumerable.Empty<NickType>())
            if (nick.Text.Trim().Length != 0) GUILayout.Label(nick.Text);
        GUILayout.EndArea();

        // Ready button
        GUILayout.BeginArea(new Rect(0f, 200f, 200f, 200f));
        if (GUILayout.Button(_ready ? "Not Ready" : "Ready"))
        {
            _ready = !_ready;
            _lobbyNetwork.ReadyUpdateServerRpc(_ready);
        }

        // Start button
        if (_startAvailable && GUILayout.Button("Start"))
        {
            NetworkController.Instance.LoadGameScene();
        }

        DisplayJoinCode();
    }

    private void DisplayJoinCode()
    {
        if (_joinCode != string.Empty)
            GUILayout.Label("Join code: " + _joinCode);
    }
}

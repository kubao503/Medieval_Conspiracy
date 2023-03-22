using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NetworkController : NetworkBehaviour
{
    public static NetworkController Instance;

    [SerializeField] private LobbyUIController _lobbyUI;
    [SerializeField] private int _maxPlayers;
    private UnityTransport _transport;
    private bool _gameStarted = false;
    private bool _connected = false;


    private async void Awake()
    {
        Instance = this;

        await Authenticate();
    }


    private void Start()
    {
        _transport = FindObjectOfType<UnityTransport>();
        NetworkManager.OnClientConnectedCallback += ConnectionSuccess;
        NetworkManager.OnClientDisconnectCallback += ConnectionFail;
        NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
    }


    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = !_gameStarted;
    }


    private void ConnectionSuccess(ulong clientId)
    {
        if (clientId == NetworkManager.LocalClientId)
        {
            _lobbyUI.CurrentState = LobbyUIController.State.NICK_CHOOSING;
            LobbyNetwork.Instance.ReadyUpdateServerRpc(ready: false);
            _connected = true;
        }
    }


    private void ConnectionFail(ulong clientId)
    {
        Debug.Log("ConnectionFail");
        Debug.Log("Client: " + IsClient + "; ClientId: " + clientId);
        Debug.Log("Client-side issue? " + (clientId == NetworkManager.LocalClientId) + "; Connected? " + _connected);

        if (clientId == NetworkManager.LocalClientId) // Client-side issue
        {
            _lobbyUI.CurrentState = LobbyUIController.State.NOT_CONNECTED;

            // Change scene
            if (_connected)
            {
                NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
                _connected = false;
            }
        }
        else // Server-side issue
        {

        }
    }


    private static async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    public async void CreateGlobalGame()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(_maxPlayers);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        LobbyUIController.Instance.SetJoinCode(joinCode);

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();
    }


    public async void JoinGlobalGame(string joinCode)
    {
        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joinCode);

        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }


    public void CreateLocalGame()
    {
        NetworkManager.Singleton.StartHost();
    }


    public void JoinLocalGame() => NetworkManager.Singleton.StartClient();


    public void LoadGameScene()
    {
        NetworkManager.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        _gameStarted = true;
    }
}

using Unity.Collections;
using Unity.Netcode;


public class LobbyPlayerData : NetworkBehaviour
{
    public static LobbyPlayerData LocalPlayer;

    private readonly NetworkVariable<bool> _netReady = new(
        value: false,
        writePerm: NetworkVariableWritePermission.Owner
    );

    private readonly NetworkVariable<Team> _netTeam = new(
        value: Team.A,
        writePerm: NetworkVariableWritePermission.Owner
    );

    private readonly NetworkVariable<FixedString64Bytes> _netNick = new(
        writePerm: NetworkVariableWritePermission.Owner
    );

    public bool Ready
    {
        get => _netReady.Value;
    }

    public Team Team
    {
        get => _netTeam.Value;
        set => _netTeam.Value = value;
    }

    public string Nick
    {
        get => _netNick.Value.ToString();
        set => _netNick.Value = value;
    }

    public void ToggleReady()
    {
        _netReady.Value = !_netReady.Value;
    }

    public static bool IsNickCorrect(string nick)
    {
        var trimmedLength = nick.Trim().Length;
        return trimmedLength != 0;
    }

    private void Awake()
    {
        LobbyPlayerDataManager.Instance.RegisterPlayer(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
            LocalPlayer = this;
    }
}

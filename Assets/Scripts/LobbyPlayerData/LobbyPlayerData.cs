using Unity.Netcode;
using UnityEngine; //debug


public class LobbyPlayerData : NetworkBehaviour
{
    public static LobbyPlayerData LocalPlayer;

    private readonly NetworkVariable<bool> _ready = new(
        value: false,
        writePerm: NetworkVariableWritePermission.Owner
    );

    public bool Ready
    {
        get => _ready.Value;
    }

    public void ToggleReady()
    {
        _ready.Value = !_ready.Value;
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

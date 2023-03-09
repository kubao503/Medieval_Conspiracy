using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using NickType = StringContainer;


/// <summary>
/// Must be placed on the same object as LobbyUIController
/// </summary>
public class LobbyNetwork : NetworkBehaviour
{
    public static LobbyNetwork Instance;

    [SerializeField] private TeamManager _teamManager;
    private LobbyUIController _lobbyUI;


    private void Awake()
    {
        Instance = this;
        _lobbyUI = GetComponent<LobbyUIController>();
    }


    [ServerRpc(RequireOwnership = false)]
    public void NickUpdateServerRpc(NickType nick, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        _teamManager.NickUpdate(nick.Text, clientId);

        _teamManager.NickTablesUpdate();
    }


    [ClientRpc]
    public void NickTablesUpdateClientRpc(NickType[] teamANicks, NickType[] teamBNicks)
    {
        _lobbyUI.NickTablesUpdate(teamANicks, teamBNicks);
    }


    [ServerRpc(RequireOwnership = false)]
    public void TeamUpdateServerRpc(Team team, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        _teamManager.TeamUpdate(team, clientId);

        _teamManager.NickTablesUpdate();
    }


    [ServerRpc(RequireOwnership = false)]
    public void ReadyUpdateServerRpc(bool ready, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        _teamManager.ReadyUpdate(ready, clientId);

        // Update start button
        var startEnabled = _teamManager.AllPlayersReady;
        _lobbyUI.StartAvailable = startEnabled;
        //_lobbyUI.StartAvailable = _teamManager.AllPlayersReady;
    }
}

public class StringContainer : INetworkSerializable
{
    public string Text;

    public StringContainer() { }

    public StringContainer(string text) => Text = text;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsWriter)
        {
            serializer.GetFastBufferWriter().WriteValueSafe(Text);
        }
        else
        {
            serializer.GetFastBufferReader().ReadValueSafe(out Text);
        }
    }

    public override bool Equals(object obj)
    {
        return Text == ((StringContainer)obj).Text;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

using NickType = StringContainer;


public enum Team : byte
{
    A,
    B,
    Total
}


public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance;

    [SerializeField] private LobbyNetwork _lobbyNetwork;

    private readonly (string, Team, bool, bool) _defaultPlayerData = ("", Team.A, false, false);
    private readonly Dictionary<ulong, (string Nick, Team Team, bool Ready, bool Dead)> _playersData = new();

    public bool AllPlayersReady => _playersData.All(x => x.Value.Ready);

    public void NickUpdate(string nick, ulong clientId)
    {
        if (!_playersData.TryGetValue(clientId, out var userData))
            userData = _defaultPlayerData;
        userData.Nick = nick;
        _playersData[clientId] = userData;
    }

    public void NickTablesUpdate()
    {
        // Might need to add Select(x => new(x)) as the first method call
        NickType[] teamANicks = _playersData.Values.Where(x => x.Team == Team.A).Select(x => new NickType(x.Nick)).ToArray();
        NickType[] teamBNicks = _playersData.Values.Where(x => x.Team == Team.B).Select(x => new NickType(x.Nick)).ToArray();

        _lobbyNetwork.NickTablesUpdateClientRpc(teamANicks, teamBNicks);
    }

    public void TeamUpdate(Team team, ulong clientId)
    {
        if (!_playersData.TryGetValue(clientId, out var userData))
            userData = _defaultPlayerData;
        userData.Team = team;
        _playersData[clientId] = userData;
    }

    public void ReadyUpdate(bool ready, ulong clientId)
    {
        //var userData = _playersData.GetValueOrDefault(clientId, _defaultPlayerData);
        if (!_playersData.TryGetValue(clientId, out var userData))
            userData = _defaultPlayerData;
        userData.Ready = ready;
        _playersData[clientId] = userData;
    }

    public void SpawnPlayers()
    {
        var connectedClientsIds = NetworkManager.Singleton.ConnectedClientsIds;
        foreach (var clientId in connectedClientsIds)
        {
            Team team = _playersData[clientId].Team;
            PlayerSpawner.Instance.Spawn(clientId, team);
        }
    }

    public void DeadPlayerUpdate(Team playerTeam, ulong clientId)
    {
        SetPlayerStateToDead(clientId);

        if (IsGameOver(playerTeam))
            EndGameClientRpc(playerTeam);
    }

    private void SetPlayerStateToDead(ulong clientId)
    {
        var deadPlayerData = _playersData[clientId];
        deadPlayerData.Dead = true;
        _playersData[clientId] = deadPlayerData;
    }

    private bool IsGameOver(Team playerTeam)
    {
        foreach (var playerData in _playersData.Values)
        {
            Debug.Log("Game continues: " + !playerData.Dead +
                " && " + (playerData.Team == playerTeam));

            // TODO: IsAlivePlayerFromTheSameTeam()
            if (!playerData.Dead && playerData.Team == playerTeam)
                return false;
        }
        return true;
    }

    [ClientRpc]
    private void EndGameClientRpc(Team loosingTeam)
    {
        Debug.Log("Received EndGameClientRpc()");
        TeamController.LocalPlayerInstance.EndGame(loosingTeam);
    }

    private void Awake()
    {
        Instance = this;
    }
}


public class InvalidTeamException : UnityException { }

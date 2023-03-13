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

    private void Awake()
    {
        Instance = this;
    }

    public void NickUpdate(string nick, ulong clientId)
    {
        if (!_playersData.TryGetValue(clientId, out var userData)) userData = _defaultPlayerData;
        userData.Nick = nick;
        _playersData[clientId] = userData;
    }

    public void NickTablesUpdate()
    {
        // Send new nick arrays to all clients
        // Might need to add Select(x => new(x)) as the first method call
        NickType[] teamANicks = _playersData.Values.Where(x => x.Team == Team.A).Select(x => new NickType(x.Nick)).ToArray();
        NickType[] teamBNicks = _playersData.Values.Where(x => x.Team == Team.B).Select(x => new NickType(x.Nick)).ToArray();

        _lobbyNetwork.NickTablesUpdateClientRpc(teamANicks, teamBNicks);
    }

    public void TeamUpdate(Team team, ulong clientId)
    {
        //_playersData.TryGetValue(clientId, out var userData);
        if (!_playersData.TryGetValue(clientId, out var userData)) userData = _defaultPlayerData;
        userData.Team = team;
        _playersData[clientId] = userData;
    }

    public void ReadyUpdate(bool ready, ulong clientId)
    {
        //var userData = _playersData.GetValueOrDefault(clientId, _defaultPlayerData);
        if (!_playersData.TryGetValue(clientId, out var userData)) userData = _defaultPlayerData;
        userData.Ready = ready;
        _playersData[clientId] = userData;
    }

    public void SpawnPlayers()
    {
        var connectedClientsIds = NetworkManager.Singleton.ConnectedClientsIds;
        foreach (var clientId in connectedClientsIds)
        {
            Team team = _playersData[clientId].Team;
            PlayerSpawner.Instance.SpawnPlayer(clientId, team);
        }
    }

    // Server-side
    public void DeadPlayerUpdate(Team playerTeam, ulong clientId)
    {
        // Set players state to dead
        var deadPlayerData = _playersData[clientId];
        deadPlayerData.Dead = true;
        _playersData[clientId] = deadPlayerData;

        // Check if game is over
        foreach (var playerData in _playersData.Values)
        {
            Debug.Log("Game continues: " + !playerData.Dead + " && " + (playerData.Team == playerTeam));
            // Search for alive player of the dead player's team
            if (!playerData.Dead && playerData.Team == playerTeam) return; // Game continues
        }

        // Game over
        EndGame(playerTeam);
    }

    private void EndGame(Team loosingTeam)
    {
        foreach (var playerData in _playersData.Values)
            EndGameClientRpc(loosingTeam);
    }

    [ClientRpc]
    public void EndGameClientRpc(Team loosingTeam)
    {
        TeamController.LocalPlayerInstance.EndGame(loosingTeam);
    }
}


public class InvalidTeamException : UnityException { }

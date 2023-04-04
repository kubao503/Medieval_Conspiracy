using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance;

    // TODO: Delete following two lines
    private readonly bool _defaultPlayerData = false;
    private readonly Dictionary<ulong, bool> _clientDead = new(); // Dead

    public void SpawnPlayers()
    {
        var connectedClientsIds = NetworkManager.Singleton.ConnectedClientsIds;
        foreach (var clientId in connectedClientsIds)
        {
            Team team = LobbyPlayerDataManager.Instance.GetClientTeam(clientId);
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
        var deadPlayerData = _clientDead[clientId];
        deadPlayerData = true;
        _clientDead[clientId] = deadPlayerData;
    }

    private bool IsGameOver(Team team)
    {
        var clientIds = LobbyPlayerDataManager.Instance.GetClientIdsFromTeam(team);
        foreach (var clientId in clientIds)
        {
            if (!_clientDead[clientId])
                return false;
        }
        return true;
    }

    [ClientRpc]
    private void EndGameClientRpc(Team loosingTeam)
    {
        TeamController.LocalPlayerInstance.EndGame(loosingTeam);
    }

    private void Awake()
    {
        Instance = this;
    }
}


public class InvalidTeamException : UnityException { }

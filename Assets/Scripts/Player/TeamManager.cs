using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance;

    private readonly HashSet<ulong> _deadClients = new();

    public void DeadPlayerUpdate(Team playerTeam, ulong clientId)
    {
        _deadClients.Add(clientId);

        if (IsGameOver(playerTeam))
            EndGameClientRpc(playerTeam);
    }

    private bool IsGameOver(Team team)
    {
        var clientsFromTeam = LobbyPlayerDataManager.Instance.GetClientIdsFromTeam(team);
        var aliveClientsFromTeam = Enumerable.Except(clientsFromTeam, _deadClients);
        return aliveClientsFromTeam.Count() == 0;
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

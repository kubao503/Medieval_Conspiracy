using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LobbyPlayerDataManager : MonoBehaviour
{
    public static LobbyPlayerDataManager Instance;
    private readonly List<LobbyPlayerData> _playersData = new();

    public void RegisterPlayer(LobbyPlayerData playerData)
    {
        _playersData.Add(playerData);
    }

    public bool AreAllPlayersReady()
    {
        return _playersData.All(x => x.Ready);
    }

    public IEnumerable<string> GetNicksOfPlayersFromTeam(Team team)
    {
        return _playersData.FindAll(p => p.Team == team).Select(p => p.Nick);
    }

    public Team GetClientTeam(ulong clientId)
    {
        var foundClient = _playersData.Find(c => c.OwnerClientId == clientId);
        return foundClient.Team;
    }

    public IEnumerable<ulong> GetClientIdsFromTeam(Team team)
    {
        var foundClients = _playersData.FindAll(c => c.Team == team);
        return foundClients.Select(c => c.OwnerClientId);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}

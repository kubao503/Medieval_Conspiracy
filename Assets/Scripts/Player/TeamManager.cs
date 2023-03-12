using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using NickType = StringContainer;

public enum Team : byte
{
    A,
    B,
    TOTAL
}


public class InvalidTeamException : UnityException { }


/// Must be placed on the same object as BaseManager.
/// Run only on server
public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance;

    [SerializeField] private LobbyNetwork _lobbyNetwork;
    [SerializeField] private GameObject _playerPrefab;

    private readonly (string, Team, bool, TeamController, bool) _defaultPlayerData = ("", Team.A, false, null, false);
    private readonly Dictionary<ulong, (string Nick, Team Team, bool Ready, TeamController TeamController, bool Dead)> _playersData = new();
    private BaseManager _baseManager;

    public bool AllPlayersReady { get => _playersData.All(x => x.Value.Ready); }

    private void Awake()
    {
        Instance = this;

        _baseManager = GetComponent<BaseManager>();
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

    private void SetTeamController(TeamController teamController, ulong clientId)
    {
        _playersData.TryGetValue(clientId, out var userData);
        userData.TeamController = teamController;
        _playersData[clientId] = userData;
    }

    // Server-side
    public void SpawnPlayers()
    {
        Debug.Log("Spawning players");
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            SpawnPlayer(clientId);

        MainUISubscriptionClientRpc();
    }

    [ClientRpc]
    private void MainUISubscriptionClientRpc()
    {
        MainUIController.Instance.SubscribeToLocalPlayerEvents();
    }

    // Server-side
    private void SpawnPlayer(ulong clientId)
    {
        // Get player team
        Team team = _playersData[clientId].Team;

        // Get position of team base entrance
        var position = _baseManager.EntrancePosition(team);

        // Spawn player inside base
        var player = Instantiate(_playerPrefab, position, Quaternion.identity);

        // Set player team
        player.GetComponent<TeamController>().Team = team;
        //player.GetComponent<PlayerController>().EnteringBuilding();

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        SetTeamController(player.GetComponent<TeamController>(), clientId);
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
        {
            playerData.TeamController.EndGameClientRpc(loosingTeam);
        }
    }
}

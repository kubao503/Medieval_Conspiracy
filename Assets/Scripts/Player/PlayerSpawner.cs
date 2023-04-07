using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] private GameObject _playerPrefab;
    private GameObject _player;

    public void SpawnAllPlayers()
    {
        var connectedClientsIds = NetworkManager.Singleton.ConnectedClientsIds;
        foreach (var clientId in connectedClientsIds)
        {
            Team team = LobbyPlayerDataManager.Instance.GetClientTeam(clientId);
            SpawnOnePlayer(clientId, team);
        }
    }

    private void SpawnOnePlayer(ulong clientId, Team team)
    {
        InstantiatePlayerInsideTeamBase(team);
        SpawnAsPlayerObjectOfClient(clientId);
        _player.GetComponent<SkinPicker>().SetNetSkin();
        SetPlayerTeam(team);
    }

    private void InstantiatePlayerInsideTeamBase(Team team)
    {
        var basePosition = BaseManager.Instance.GetBasePosition(team);
        _player = Instantiate(_playerPrefab, basePosition, Quaternion.identity);
    }

    private void SpawnAsPlayerObjectOfClient(ulong clientId)
    {
        var networkObject = _player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
    }

    private void SetPlayerTeam(Team team)
    {
        var teamController = _player.GetComponent<TeamController>();
        teamController.Team = team;
    }

    private void Awake()
    {
        Instance = this;
    }
}

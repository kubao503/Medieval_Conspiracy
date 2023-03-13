using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] private GameObject _playerPrefab;
    private GameObject _player;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnPlayer(ulong clientId, Team team)
    {
        InstantiatePlayerInsideTeamBase(team);
        SetPlayerTeam(team);
        SpawnAsPlayerObjectOfClient(clientId);
    }

    private void InstantiatePlayerInsideTeamBase(Team team)
    {
        var basePosition = BaseManager.Instance.GetBasePosition(team);
        _player = Instantiate(_playerPrefab, basePosition, Quaternion.identity);
    }

    private void SetPlayerTeam(Team team)
    {
        var teamController = _player.GetComponent<TeamController>();
        teamController.Team = team;
    }

    private void SpawnAsPlayerObjectOfClient(ulong clientId)
    {
        var networkObject = _player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
    }
}

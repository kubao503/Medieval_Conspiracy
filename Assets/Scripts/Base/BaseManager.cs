using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BaseManager : NetworkBehaviour
{
    public static BaseManager Instance;

    [SerializeField] private GameObject _teamEntrancePrefab;
    private Transform _buildingHolder; // TODO: Make Serialized
    private Vector3[] _entrancePositions = new Vector3[(int)Team.Total];

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoadCompletedCallback;

        base.OnNetworkSpawn();
    }

    public Vector3 GetBasePosition(Team team)
    {
        return _entrancePositions[(int)team];
    }

    private void SceneLoadCompletedCallback(
        string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SetBases();
        TeamManager.Instance.SpawnPlayers();
    }

    private void SetBases()
    {
        FindAndSetBuildingHolder();
        IList<Transform> entrances = FindAllEntrances();

        CheckEntranceCount(entrances);

        SetTeamBaseAtRandomEntrance(entrances, Team.A);
        SetTeamBaseAtRandomEntrance(entrances, Team.B);
    }

    private void FindAndSetBuildingHolder()
    {
        _buildingHolder = GameObject.Find("Building Holder").transform;
    }

    private List<Transform> FindAllEntrances()
    {
        List<Transform> entrances = new();
        foreach (Transform subHolder in _buildingHolder)
        {
            foreach (Transform building in subHolder)
            {
                // This could be building without entrance
                if (TryFindEntrace(building, out var entrance))
                    entrances.Add(entrance);
            }
        }
        return entrances;
    }

    private bool TryFindEntrace(Transform building, out Transform entranceOut)
    {
        var entrance = building.Find("Entrance");
        if (entrance != null)
        {
            var rug = entrance.Find("Rug");
            if (rug != null)
            {
                entranceOut = rug;
                return true;
            }
        }
        entranceOut = null;
        return false;
    }

    private void CheckEntranceCount(IList<Transform> entrances)
    {
        Debug.Assert(entrances.Count >= (int)Team.Total, "Too few entrances found");
    }

    private void SetTeamBaseAtRandomEntrance(IList<Transform> entrances, Team team)
    {
        Transform entrance = PopRandomEntrace(entrances);
        SetTeamEntrancePrefab(entrance, team);
        UpdateEntrancePositions(entrance, team);
    }

    private Transform PopRandomEntrace(IList<Transform> entrances)
    {
        var index = UnityEngine.Random.Range(0, entrances.Count);
        Transform entrance = entrances[index];
        entrances.RemoveAt(index);
        return entrance;
    }

    private void SetTeamEntrancePrefab(Transform entrance, Team team)
    {
        var teamEntrance = Instantiate(_teamEntrancePrefab, entrance.position, entrance.rotation);
        teamEntrance.GetComponent<NetworkObject>().Spawn(true);
        teamEntrance.GetComponent<BaseController>().Team = team;
    }

    private void UpdateEntrancePositions(Transform entrance, Team team)
    {
        _entrancePositions[(int)team] = entrance.position;
    }
}

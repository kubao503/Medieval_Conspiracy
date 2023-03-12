using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Must be placed on the same object as TeamManager.
/// Run only on server
/// </summary>
public class BaseManager : MonoBehaviour, IServerOnly
{
    [SerializeField] private GameObject _teamEntrancePrefab;
    private Transform _buildingHolder;
    private Vector3 _entrancePosA;
    private Vector3 _entrancePosB;


    // IServerOnly interface
    public void OnClient() => Destroy(this);
    public void OnServer() => NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SetBases;


    public Vector3 EntrancePosition(Team team)
    {
        try
        {
            return team switch
            {
                Team.A => _entrancePosA,
                Team.B => _entrancePosB,
                _ => throw new InvalidTeamException()
            };
        }
        catch (NullReferenceException)
        {
            Debug.Log("Bases not set");
            return Vector3.zero;
        }
    }


    // Server-side
    private void SetBases(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        _buildingHolder = GameObject.Find("Building Holder").transform;

        List<Transform> entrances = new();
        foreach (Transform subHolder in _buildingHolder)
        {
            foreach (Transform building in subHolder)
            {
                // This could be building without entrance
                var entrance = building.Find("Entrance");
                if (entrance != null)
                {
                    var rug = entrance.Find("Rug");
                    if (rug != null) entrances.Add(rug);
                }
            }
        }

        // Too few entrances
        if (entrances.Count < (int)Team.TOTAL) { Debug.LogError("Too few entrances found"); return; }

        // Adding BaseControllers for two chosen buildings
        SetRandomBase(entrances, Team.A);
        SetRandomBase(entrances, Team.B);
    }


    // Server-side
    private void SetRandomBase(List<Transform> entrances, Team team)
    {
        // Base index
        var index = UnityEngine.Random.Range(0, entrances.Count);
        Transform entranceTransform = entrances[index];
        entrances.RemoveAt(index);


        // Adding team base entrace prefab
        var teamEntrance = Instantiate(_teamEntrancePrefab, entranceTransform.position, entranceTransform.rotation);
        teamEntrance.GetComponent<BaseController>().Team = team;
        teamEntrance.GetComponent<NetworkObject>().Spawn(true);

        //Debug.Log("Base position " + teamEntrance.transform.position);
        //Debug.Break();

        //entrance.gameObject.GetComponent<BaseController>().Team = team;

        switch (team)
        {
            case Team.A: _entrancePosA = entranceTransform.position; break;
            case Team.B: _entrancePosB = entranceTransform.position; break;
            default: throw new InvalidTeamException();
        }

        //Debug.Log(entrances.Count + " entrances left");
    }
}

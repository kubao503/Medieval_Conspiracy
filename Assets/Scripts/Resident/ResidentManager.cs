using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using ResidentScript = ResidentController;


public class ResidentManager : NetworkBehaviour
{
    public static ResidentManager Instance;

    private readonly HashSet<ResidentScript> _spawnedResidents = new();

    [SerializeField] private GameObject _residentPrefab;
    [SerializeField] private int _residentCount;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Instance = this;
            NetworkManager.SceneManager.OnLoadEventCompleted += SpawnRandomResidents;
            StartCoroutine(DistanceSyncCoroutine());
        }
        else
            enabled = false;
    }

    private void SpawnRandomResidents(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SpawnRandomResidents();
    }

    private void SpawnRandomResidents()
    {
        for (int i = 0; i < _residentCount; ++i)
            SpawnResident();
    }

    IEnumerator DistanceSyncCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            foreach (var resident in _spawnedResidents)
                resident.DistanceSync();
        }
    }

    private void SpawnResident()
    {
        var newResident = Instantiate(_residentPrefab);

        newResident.GetComponent<NetworkObject>().Spawn();

        // Add resident to list
        _spawnedResidents.Add(newResident.GetComponent<ResidentScript>());
    }

    public void SetSpeedToDefault()
    {
        foreach (var resident in _spawnedResidents) resident.SetSpeedToDefault();
    }
}

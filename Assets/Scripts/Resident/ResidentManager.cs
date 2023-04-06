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


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Instance = this;
            StartCoroutine(DistanceSyncCoroutine());
        }
        else
            enabled = false;
    }

    public void SpawnRandomResidents()
    {
        while (SkinManager.Instance.SkinsLeft > 0)
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
        newResident.GetComponent<SkinPicker>().SetNetSkin();

        // Add resident to list
        _spawnedResidents.Add(newResident.GetComponent<ResidentScript>());
    }

    public void SetSpeedToDefault()
    {
        foreach (var resident in _spawnedResidents) resident.SetSpeedToDefault();
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using ResidentScript = ResidentController;


public class ResidentManager : NetworkBehaviour
{
    public static ResidentManager Instance;

    [SerializeField] private GameObject _residentPrefab;
    [SerializeField] private float _distanceSyncInterval;
    private WaitForSeconds _distanceSyncWait;
    private readonly HashSet<ResidentScript> _spawnedResidents = new();

    public void SetSpeedToDefault()
    {
        foreach (var resident in _spawnedResidents)
            resident.SetSpeedToDefault();
    }

    public void SpawnRandomResidents()
    {
        while (SkinManager.Instance.SkinsLeft > 0)
            SpawnResident();
    }

    private void SpawnResident()
    {
        var newResident = Instantiate(_residentPrefab);

        newResident.GetComponent<NetworkObject>().Spawn();
        newResident.GetComponent<SkinPicker>().SetNetSkin();

        _spawnedResidents.Add(newResident.GetComponent<ResidentScript>());
    }

    private void Awake()
    {
        _distanceSyncWait = new WaitForSeconds(_distanceSyncInterval);
    }

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

    private IEnumerator DistanceSyncCoroutine()
    {
        while (true)
        {
            yield return _distanceSyncWait;
            foreach (var resident in _spawnedResidents)
                resident.DistanceSync();
        }
    }
}

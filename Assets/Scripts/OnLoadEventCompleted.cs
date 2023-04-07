using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Netcode;


public class OnLoadEventCompleted : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoadCompletedCallback;
    }

    private void SceneLoadCompletedCallback(
        string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        BaseManager.Instance.SetBases();
        PlayerSpawner.Instance.SpawnAllPlayers();
        ResidentManager.Instance.SpawnRandomResidents();
    }
}

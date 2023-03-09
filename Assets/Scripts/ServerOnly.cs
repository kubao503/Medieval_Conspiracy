using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;


public interface IServerOnly
{
    void OnClient();
    void OnServer();
}


public class ServerOnly : NetworkBehaviour
{
    [SerializeField] private List<Object> _serverOnlyScripts;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            foreach (var script in _serverOnlyScripts) ((IServerOnly)script).OnServer();
        else
            foreach (var script in _serverOnlyScripts) ((IServerOnly)script).OnClient();

        base.OnNetworkSpawn();
    }
}

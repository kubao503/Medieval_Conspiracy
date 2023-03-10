using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


interface INetworkSpawn
{
    void OnNetworkSpawn(bool IsOwner, bool IsServer);
}


public class NetworkSpawn : NetworkBehaviour
{
    [SerializeField] private List<INetworkSpawn> _scripts = new();
    public override void OnNetworkSpawn()
    {
        foreach (var script in _scripts)
            script.OnNetworkSpawn(IsOwner, IsServer);
    }
}

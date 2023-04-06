using UnityEngine;
using Unity.Netcode;


public interface INetworkManager
{
    bool IsServer { get; }
}


public class RealNetworkManger : INetworkManager
{
    private NetworkManager _networkManager = NetworkManager.Singleton;

    public bool IsServer => _networkManager.IsServer;
}


public class FakeNetworkManger : INetworkManager
{
    public bool IsServer { get; set; }

    public FakeNetworkManger()
    {
        Debug.LogWarning("Switching to FakeNetworkManger");
    }
}

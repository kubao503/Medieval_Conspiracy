using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LobbyPlayerDataManager : MonoBehaviour
{
    public static LobbyPlayerDataManager Instance;
    private readonly List<LobbyPlayerData> _playersData = new();

    public bool AreAllPlayersReady()
    {
        return _playersData.All(x => x.Ready);
    }

    public void RegisterPlayer(LobbyPlayerData playerData)
    {
        _playersData.Add(playerData);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using NPCScript = NPCController;

public class NPCManager : NetworkBehaviour
{
    public static NPCManager Instance;

    private readonly HashSet<NPCScript> _spawnedNPCs = new();

    [SerializeField] private Transform Spawnpoint;
    //public PathCreator pathCreator;
    [SerializeField] private GameObject NpcPrefab;
    [SerializeField] private int _npcNumber;
    //[SerializeField] private float _panicDistance;




    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Instance = this;
            NetworkManager.SceneManager.OnLoadEventCompleted += SpawnRandomNPCs;
            StartCoroutine(DistanceSyncCoroutine());
            //SpawnRandomNPCs();
        }
        else
            enabled = false;
    }

    private void SpawnRandomNPCs(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        SpawnRandomNPCs();
    }


    // Executed only by server
    private void SpawnRandomNPCs()
    {
        for (int i = 0; i < _npcNumber; ++i)
            SpawnNPC();
    }


    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(
    //        Spawnpoint.position + Spawnpoint.TransformDirection(new Vector3(-_spawnRange, 0f, 0f)),
    //        Spawnpoint.position + Spawnpoint.TransformDirection(new Vector3(_spawnRange, 0f, 0f))
    //    );
    //}


    IEnumerator DistanceSyncCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            foreach (var npc in _spawnedNPCs) npc.DistanceSync();
        }
    }


    private void SpawnNPC()
    {
        var newNpc = Instantiate(NpcPrefab);

        newNpc.GetComponent<NetworkObject>().Spawn();

        // Add NPC to list
        _spawnedNPCs.Add(newNpc.GetComponent<NPCScript>());
    }


    //public void Panic(in HashSet<Transform> dangerPoints)
    //{
    //    var spans = dangerPoints.Select(pos => new Span(pathCreator.path.GetClosestDistanceAlongPath(pos.position), _panicDistance)).ToArray();
    //    foreach (var npc in _spawnedNPCs) npc.Panic(Span.MergeSpans(spans));
    //}


    /// <summary>
    /// Makes all NPC walk with their default speed
    /// </summary>
    public void Walk()
    {
        foreach (var npc in _spawnedNPCs) npc.SetSpeedToDefault();
    }


    public void RemoveFromNpc(GameObject npc)
    {
        _spawnedNPCs.Remove(npc.GetComponent<NPCScript>());
    }
}

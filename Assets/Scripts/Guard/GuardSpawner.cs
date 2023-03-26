using UnityEngine;
using Unity.Netcode;


public class GuardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _guardPrefab;
    [SerializeField] private float _spawnRadius;

    public GameObject Spawn(float playerDistanceAlongPath)
    {
        var position = GetRandomPositionAroundPlayer(playerDistanceAlongPath);
        var guard = Instantiate(_guardPrefab, position, Quaternion.identity);

        SpawnInstantiatedGuard(guard);

        return guard;
    }

    private Vector3 GetRandomPositionAroundPlayer(float playerDistanceAlongPath)
    {
        var distance = GetRandomDistanceAlongPathAroundPlayer(playerDistanceAlongPath);

        var position = MainPath.Path.GetPointAtDistance(distance);
        position.y = _guardPrefab.transform.position.y;

        return position;
    }

    private float GetRandomDistanceAlongPathAroundPlayer(float playerDistanceAlongPath)
    {
        if (Random.value < .5)
            return playerDistanceAlongPath + _spawnRadius;
        return playerDistanceAlongPath - _spawnRadius;
    }

    private void SpawnInstantiatedGuard(GameObject guard)
    {
        var networkObject = guard.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }
}

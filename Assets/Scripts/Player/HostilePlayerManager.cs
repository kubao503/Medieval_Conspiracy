using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;


public class HostilePlayerManager : NetworkBehaviour
{
    public static HostilePlayerManager Instance;

    private readonly HashSet<Transform> _hostilePlayers = new();

    [SerializeField] private LayerMask _residentLayer;
    [SerializeField] private float _panicRadius = 1f;
    [SerializeField] private float _residentPanicInterval = 1f;
    private WaitForSeconds _residentPanicWait;
    private Coroutine _residentPanicCoroutine;

    public bool IsPlayerHostile(Transform player)
    {
        return _hostilePlayers.Contains(player);
    }

    public bool GetHostilePlayerPositionIfThereAreAny(out Vector3 playerPosition)
    {
        playerPosition = Vector3.zero;
        if (_hostilePlayers.Count == 0)
            return false;

        var firstPlayer = _hostilePlayers.First();
        playerPosition = firstPlayer.position;
        return true;
    }

    public void AddToHostilePlayers(Transform player)
    {
        TryStartResidentPanicCoroutine();
        _hostilePlayers.Add(player);
        GuardManager.Instance.TryStartRaid(player.position);
    }

    private void TryStartResidentPanicCoroutine()
    {
        if (_hostilePlayers.Count == 0)
            _residentPanicCoroutine = StartCoroutine(ResidentPanicCoroutine());
    }

    IEnumerator ResidentPanicCoroutine()
    {
        while (true)
        {
            ResidentManager.Instance.SetSpeedToDefault();

            foreach (var player in _hostilePlayers)
            {
                var residentsInDanger = Physics.OverlapSphere(player.position, _panicRadius, _residentLayer);

                foreach (var resident in residentsInDanger)
                    resident.gameObject.GetComponent<ResidentController>().Panic(player.position);
            }

            yield return _residentPanicWait;
        }
    }

    public void RemoveFromHostilePlayers(Transform player)
    {
        if (_hostilePlayers.Remove(player))
            CheckForNoHostilePlayers();
    }

    public void RemoveNonHostilePlayers()
    {
        _hostilePlayers.RemoveWhere(x => !x.GetComponent<PlayerHostility>().IsHostile);
        CheckForNoHostilePlayers();
    }

    private void CheckForNoHostilePlayers()
    {
        if (_hostilePlayers.Count == 0)
        {
            GuardManager.Instance.TryEndRaid();
            ResidentManager.Instance.SetSpeedToDefault();
            StopCoroutine(_residentPanicCoroutine);
        }
    }

    public Transform GetClosestHostilePlayer(Vector3 guardPosition)
    {
        Transform closestPlayer = null;
        float closestDistanceToPlayer = float.PositiveInfinity;

        foreach (var player in _hostilePlayers)
        {
            var distanceToPlayer = Vector3.Distance(guardPosition, player.position);

            if (distanceToPlayer < closestDistanceToPlayer)
            {
                closestPlayer = player;
                closestDistanceToPlayer = distanceToPlayer;
            }
        }

        return closestPlayer;
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Instance = this;
            _residentPanicWait = new(_residentPanicInterval);
        }
    }
}

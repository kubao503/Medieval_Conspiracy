using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


// Server-side
public class HostilePlayerManager : NetworkBehaviour
{
    public static HostilePlayerManager Instance;

    // Players currently attacked by guards
    private readonly HashSet<Transform> _hostilePlayers = new();

    private Coroutine _residentPanicCo;
    [SerializeField] private LayerMask _residentLayer;
    [SerializeField] private float _residentRunAwayFrequency;
    [SerializeField] private float _panicRadius = 1f;


    private void Awake()
    {
        Instance = this;
    }


    public override void OnNetworkSpawn()
    {
        // Let this script run only on server
        if (!IsServer)
            enabled = false;
        else
            Instance = this;
    }


    public bool IsPlayerHostile(Transform player) => _hostilePlayers.Contains(player);


    public bool CheckForHostilePlayers(out Vector3 playerPosition)
    {
        playerPosition = Vector3.zero;
        if (_hostilePlayers.Count == 0) return false;

        // Get random player from _hostilePlayers
        var enumerator = _hostilePlayers.GetEnumerator();
        enumerator.MoveNext();
        playerPosition = enumerator.Current.transform.position;
        return true;
    }


    public void AddToHostilePlayers(Transform player)
    {
        // First hostile player
        if (_hostilePlayers.Count == 0) _residentPanicCo = StartCoroutine(ResidentPanicCo());

        _hostilePlayers.Add(player);

        // Start guard raid
        GuardManager.Instance.TryStartRaid(player.position);

        //Debug.Log("Hostile players count (Add): " + _hostilePlayers.Count);
    }


    public void RemoveFromHostilePlayers(Transform player)
    {
        if (_hostilePlayers.Remove(player)) CheckForNoTargets();

        //Debug.Log("Hostile players count (Rem): " + _hostilePlayers.Count);
    }


    public void RemoveNonHostilePlayers()
    {
        _hostilePlayers.RemoveWhere(x => !x.GetComponent<PlayerHostility>().IsHostile);
        CheckForNoTargets();
    }


    private void CheckForNoTargets()
    {
        if (_hostilePlayers.Count == 0)
        {
            GuardManager.Instance.TryEndRaid();

            // Stop panic
            ResidentManager.Instance.Walk();
            StopCoroutine(_residentPanicCo);
        }
    }


    /// <summary>
    /// Returns hostile player that is closest to guardPosition
    /// </summary>
    /// <param name="guardPosition">Postion of the guard</param>
    /// <returns></returns>
    public Transform ClosestTarget(Vector3 guardPosition)
    {
        Transform closestTarget = null;
        float closestTargetDistance = float.PositiveInfinity;

        // Iterate over all raidTargets
        foreach (var target in _hostilePlayers)
        {
            var targetPosition = target.transform.position;

            // For each calculate its distance
            var targetDistance = Vector3.Distance(guardPosition, targetPosition);

            if (targetDistance < closestTargetDistance)
            {
                closestTarget = target;
                closestTargetDistance = targetDistance;
            }
        }

        // Return the closest one as a target
        return closestTarget;
    }


    IEnumerator ResidentPanicCo()
    {
        while (true)
        {
            ResidentManager.Instance.Walk();

            foreach (var player in _hostilePlayers)
            {
                // Find all resident in given panic radius
                var residentList = Physics.OverlapSphere(player.position, _panicRadius, _residentLayer);

                // Make each resident run
                foreach (var resident in residentList)
                {
                    resident.gameObject.GetComponent<ResidentController>().Panic(player.position);
                }
            }

            yield return new WaitForSeconds(1f / _residentRunAwayFrequency);
        }
    }
}

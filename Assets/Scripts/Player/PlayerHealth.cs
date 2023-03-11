using UnityEngine;
using Unity.Netcode;

using HealthType = System.Int16;

public class PlayerHealth : Mortal
{
    private readonly NetworkVariable<HealthType> _netHealth = new();

    [SerializeField] private PlayerController _playerController;
    private PlayerState _playerState;
    private PlayerHostility _playerHostility;

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _playerHostility = GetComponent<PlayerHostility>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _netHealth.OnValueChanged += HealthUpdate;
            HealthUpdate((HealthType)DefaultHealth, (HealthType)DefaultHealth);
        }

        if (IsServer) _netHealth.Value = (HealthType)DefaultHealth;
    }


    public void HealthUpdate(HealthType _, HealthType newHealth) => MainUIController.Instance.UpdateHealthText(newHealth);
    //public void HealthUpdate(HealthType _, HealthType newHealth) { Debug.Log("Inside Health update: " + newHealth);  MainUIController.Instance.UpdateHealthText(newHealth); }


    // Server-side
    protected override void Die()
    {
        HostilePlayerManager.Instance.RemoveFromHostilePlayers(transform);
        _playerHostility.StopHostileTimer();

        _playerController.Disappear();
        _playerState.CurrentState = PlayerState.State.DEAD;

        TeamManager.Instance.DeadPlayerUpdate(GetComponent<TeamController>().Team, OwnerClientId);
    }


    public override void TakeDamage(int damage)
    {
        DamageTakenClientRpc();

        base.TakeDamage(damage);

        _netHealth.Value = (HealthType)Health;
    }


    [ClientRpc]
    private void DamageTakenClientRpc()
    {
        if (IsOwner) MainUIController.Instance.ShowDamageVignete();
    }


    [ServerRpc]
    public override void RegainHealthServerRpc()
    {
        Health = DefaultHealth;

        _netHealth.Value = (HealthType)DefaultHealth;
    }
}

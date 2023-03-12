using System;
using Unity.Netcode;
using UnityEngine;

using HealthType = System.Int16;


public abstract class HealthController : NetworkBehaviour
{
    public event EventHandler<DeadEventArgs> DeadUpdated;

    private const HealthType _defaultHealth = 100;

    protected abstract HealthType Health { get; set; }
    public abstract bool IsDead { get; }

    protected void DeadNotification()
    {
        var args = new DeadEventArgs()
        {
            IsDead = this.IsDead
        };
        DeadUpdated?.Invoke(this, args);
    }

    private void Start()
    {
        if (IsServer)
            this.Health = _defaultHealth;
    }

    [ServerRpc]
    public void RegainHealthServerRpc() => RegainHealth();

    // Server-side
    public void RegainHealth()
    {
        this.Health = _defaultHealth;
    }

    // Server-side
    public void TakeDamage(int damage)
    {
        var newHealth = this.Health - damage;
        this.Health = (HealthType)Mathf.Max(0, newHealth);
    }
}


public class DeadEventArgs : EventArgs
{
    public bool IsDead { get; set; }
}

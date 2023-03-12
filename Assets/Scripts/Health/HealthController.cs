using System;
using Unity.Netcode;
using UnityEngine;

using HealthType = System.Int16;


public abstract class HealthController : MonoBehaviour
{
    private const HealthType _defaultHealth = 100;
    protected IHealthVar _health;

    public event EventHandler<DeadEventArgs> DeadUpdated
    {
        add => _health.DeadUpdated += value;
        remove => _health.DeadUpdated -= value;
    }

    public bool IsDead
    {
        get => _health.IsDead;
    }

    private void Start()
    {
        this._health.Value = _defaultHealth;
    }

    [ServerRpc]
    public void RegainHealthServerRpc() => RegainHealth();

    // Server-side
    public void RegainHealth()
    {
        this._health.Value = _defaultHealth;
    }

    // Server-side
    public void TakeDamage(int damage)
    {
        var newHealth = this._health.Value - damage;
        this._health.Value = (HealthType)Mathf.Max(0, newHealth);
    }
}

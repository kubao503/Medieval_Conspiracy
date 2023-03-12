using System;
using Unity.Netcode;
using UnityEngine;

using HealthType = System.Int16;


public abstract class HealthController : MonoBehaviour
{
    private const HealthType _defaultHealth = 100;
    protected IHealthVar _health;

    public event EventHandler Died
    {
        add => _health.Died += value;
        remove => _health.Died -= value;
    }

    public bool IsDead => _health.Value == 0;

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

using System;
using Unity.Netcode;
using UnityEngine;

using HealthType = System.Int16;


public interface IHealthVar
{
    event EventHandler Died;
    HealthType Value { get; set; }
}


public class NetDeadVar : IHealthVar
{
    public event EventHandler Died;

    private readonly NetworkVariable<bool> _netDead = new(false);
    private HealthType _health;

    public HealthType Value
    {
        get => _health;
        set
        {
            _health = value;
            _netDead.Value = IsDead();
        }
    }

    private bool IsDead()
    {
        return _health == 0;
    }

    public NetDeadVar()
    {
        _netDead.OnValueChanged += DeathUpdate;
    }

    private void DeathUpdate(bool previous, bool current)
    {
        if (current)
            Died?.Invoke(this, EventArgs.Empty);
    }
}


public class NetHealthVar : IHealthVar
{
    public event EventHandler Died;
    public event EventHandler HealthUpdated;

    private NetworkVariable<HealthType> _netHealth = new();

    public short Value
    {
        get => _netHealth.Value;
        set => _netHealth.Value = value;
    }

    public NetHealthVar()
    {
        _netHealth.OnValueChanged += HealthUpdate;
    }

    private void HealthUpdate(HealthType previous, HealthType  current)
    {
        HealthUpdated?.Invoke(this, EventArgs.Empty);
        if (current == 0)
            Died?.Invoke(this, EventArgs.Empty);
    }
}


// TODO: Change to MonoBehaviour
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

    private void Awake()
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

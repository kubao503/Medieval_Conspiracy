using System;
using Unity.Netcode;
using UnityEngine;

using HealthType = System.Int16;


public interface IHealthVar
{
    event EventHandler<DeadEventArgs> DeadUpdated;
    HealthType Value { get; set; }
    bool IsDead { get; }
}


public class NetDeadVar : IHealthVar
{
    public event EventHandler<DeadEventArgs> DeadUpdated;

    private readonly NetworkVariable<bool> _netDead = new(false);
    private HealthType _health;

    public HealthType Value
    {
        get => _health;
        set
        {
            _health = value;
            _netDead.Value = _health == 0;
        }
    }

    public bool IsDead => _netDead.Value;

    public NetDeadVar()
    {
        _netDead.OnValueChanged += DeathUpdate;
    }

    private void DeathUpdate(bool previous, bool current)
    {
        var args = new DeadEventArgs()
        {
            IsDead = current
        };
        DeadUpdated?.Invoke(this, args);
    }
}


public class NetHealthVar : IHealthVar
{
    public event EventHandler<DeadEventArgs> DeadUpdated;
    public event EventHandler<HealthEventArgs> HealthUpdated;

    private NetworkVariable<HealthType> _netHealth = new();

    public HealthType Value
    {
        get => _netHealth.Value;
        set => _netHealth.Value = value;
    }

    public bool IsDead => _netHealth.Value == 0;

    public NetHealthVar()
    {
        _netHealth.OnValueChanged += HealthUpdate;
    }

    private void HealthUpdate(HealthType oldHealth, HealthType  newHealth)
    {
        var healthArgs = GetHealthEventArgs(oldHealth, newHealth);
        HealthUpdated?.Invoke(this, healthArgs);

        if (IsDeadUpdate(oldHealth, newHealth))
        {
            var deadArgs = GetDeadEventArgs(newHealth);
            DeadUpdated?.Invoke(this, deadArgs);
        }
    }

    private HealthEventArgs GetHealthEventArgs(HealthType oldHealth, HealthType  newHealth)
    {
        return new HealthEventArgs()
        {
            OldHealth = oldHealth,
            NewHealth = newHealth
        };
    }

    private bool IsDeadUpdate(HealthType oldHealth, HealthType  newHealth)
    {
        return oldHealth == 0 || newHealth == 0;
    }

    private DeadEventArgs GetDeadEventArgs(HealthType newHealth)
    {
        return new DeadEventArgs()
        {
            IsDead = newHealth == 0
        };
    }
}


public class HealthEventArgs : EventArgs
{
    public HealthType OldHealth { get; set; }
    public HealthType NewHealth { get; set; }
}


public class DeadEventArgs : EventArgs
{
    public bool IsDead { get; set; }
}

using System;
using Unity.Netcode;

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
    public event EventHandler<HealthEventArgs> HealthUpdated;

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

    private void HealthUpdate(HealthType oldHealth, HealthType  newHealth)
    {
        var args = GetHealthEventArgs(oldHealth, newHealth);
        HealthUpdated?.Invoke(this, args);

        if (newHealth == 0)
            Died?.Invoke(this, EventArgs.Empty);
    }

    private HealthEventArgs GetHealthEventArgs(HealthType oldHealth, HealthType  newHealth)
    {
        return new HealthEventArgs()
        {
            OldHealth = oldHealth,
            NewHealth = newHealth
        };
    }
}


public class HealthEventArgs : EventArgs
{
    public HealthType OldHealth { get; set; }
    public HealthType NewHealth { get; set; }
}

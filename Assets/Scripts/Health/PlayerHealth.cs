using System;
using Unity.Netcode;

using HealthType = System.Int16;


public class PlayerHealth : HealthController
{
    public event EventHandler<HealthEventArgs> HealthUpdated;

    private NetworkVariable<HealthType> _netHealth = new();

    protected override HealthType Health
    {
        get => _netHealth.Value;
        set => _netHealth.Value = value;
    }

    public override bool IsDead => _netHealth.Value == 0;

    public PlayerHealth()
    {
        _netHealth.OnValueChanged += HealthUpdate;
    }

    private void HealthUpdate(HealthType oldHealth, HealthType  newHealth)
    {
        HealthNotification(oldHealth, newHealth);

        if (IsDeadUpdated(oldHealth, newHealth))
            base.DeadNotification();
    }

    private void HealthNotification(HealthType oldHealth, HealthType  newHealth)
    {
        var args = new HealthEventArgs()
        {
            OldHealth = oldHealth,
            NewHealth = newHealth
        };
        HealthUpdated?.Invoke(this, args);
    }

    private bool IsDeadUpdated(HealthType oldHealth, HealthType  newHealth)
    {
        return oldHealth == 0 || newHealth == 0;
    }
}


public class HealthEventArgs : EventArgs
{
    public HealthType OldHealth { get; set; }
    public HealthType NewHealth { get; set; }
}

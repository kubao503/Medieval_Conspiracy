using System;
using Unity.Netcode;

using HealthType = System.Int16;


public class PlayerHealth : HealthController
{
    public event EventHandler<HealthEventArgs> HealthUpdated;

    private NetworkVariable<HealthType> _netHealth = new();
    private PlayerState _playerState;

    protected override HealthType Health
    {
        get => _netHealth.Value;
        set => _netHealth.Value = value;
    }

    public override bool IsDead => _netHealth.Value == 0;

    private void Awake()
    {
        _netHealth.OnValueChanged += HealthUpdate;
        _playerState = GetComponent<PlayerState>();
    }

    private void HealthUpdate(HealthType oldHealth, HealthType newHealth)
    {
        HealthNotification(oldHealth, newHealth);

        if (IsDeadUpdated(oldHealth, newHealth))
        {
            _playerState.DeadUpdate(IsDead);
            base.DeadNotification();
        }
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

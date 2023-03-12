using Unity.Netcode;

using HealthType = System.Int16;


public class NpcHealth : HealthController
{
    private readonly NetworkVariable<bool> _netDead = new(false);
    private HealthType _health;

    protected override HealthType Health
    {
        get => _health;
        set
        {
            _health = value;
            _netDead.Value = _health == 0;
        }
    }

    public override bool IsDead => _netDead.Value;

    public NpcHealth()
    {
        _netDead.OnValueChanged += DeadUpdate;
    }

    private void DeadUpdate(bool previous, bool current)
    {
        base.DeadNotification();
    }
}

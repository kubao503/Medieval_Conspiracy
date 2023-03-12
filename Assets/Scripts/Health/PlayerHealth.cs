using System;

public class PlayerHealth : HealthController
{
    public event EventHandler<HealthEventArgs> HealthUpdated
    {
        add => ((NetHealthVar)_health).HealthUpdated += value;
        remove => ((NetHealthVar)_health).HealthUpdated -= value;
    }

    public PlayerHealth()
    {
        _health = new NetHealthVar();
    }
}

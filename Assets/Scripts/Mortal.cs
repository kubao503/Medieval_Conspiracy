using Unity.Netcode;
using UnityEngine;

public abstract class Mortal : NetworkBehaviour
{
    [SerializeField] private int defaultHealth;
    private int health;

    protected void Start()
    {
        Debug.Assert(this.defaultHealth > 0f, "Default health must be positive");
        this.health = this.defaultHealth;
    }

    protected int Health
    {
        get => this.health;
        set => this.health = value;
    }

    protected int DefaultHealth
    {
        get => this.defaultHealth;
    }

    // Server-side
    public virtual void TakeDamage(int damage)
    {
        if (IsDying(damage))
            Die();
        loseHealth(damage);
    }

    private bool IsDying(int damage)
    {
        return this.health > 0 && damage >= this.health;
    }

    private void loseHealth(int damage)
    {
        this.health -= damage;
        this.health = Mathf.Max(0, this.health);
    }

    [ServerRpc]
    public virtual void RegainHealthServerRpc() => RegainHealth();

    // Server-side
    public void RegainHealth()
    {
        this.health = this.defaultHealth;
    }

    // Server-side
    protected abstract void Die();
}

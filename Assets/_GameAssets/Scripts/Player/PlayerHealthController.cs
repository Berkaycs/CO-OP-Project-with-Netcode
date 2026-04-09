using UnityEngine;
using Unity.Netcode;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField] private int _health;
    [SerializeField] private int _maxHealth;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _health = _maxHealth;
        }
    }

    public int GetHealth()
    {
        return _health;
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        _health = Mathf.Clamp(_health, 0, _maxHealth);
        HealthUI.Instance.SetHealth(_health, _maxHealth);
    }

    public void RestartHealth()
    {
        _health = _maxHealth;
        HealthUI.Instance.SetHealth(_health, _maxHealth);
    }
}

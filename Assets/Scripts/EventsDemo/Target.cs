using UnityEngine;

public class Target : MonoBehaviour
{
    private const int MAX_HEALTH = 100;
    
    [SerializeField] private int health = MAX_HEALTH;

    public void TakeDamage(int amount)
    {
        health = Mathf.Clamp(health - amount, 0, MAX_HEALTH);
        Debug.Log($"Target health: {health}");
    }
}

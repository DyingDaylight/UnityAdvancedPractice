using UnityEngine;
using UnityEngine.InputSystem;

public class Turret : MonoBehaviour
{
    [SerializeField] private ProjectileManager projectileManager;
    
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Shoot();
        }
    }
    
    private void Shoot()
    {
        projectileManager.Spawn(projectilePrefab, firePoint.position, firePoint.rotation,firePoint.forward);
    }
}

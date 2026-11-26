using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private ImpactManager impactManager;
    [SerializeField] private HitStatistics hitStatistics;
    
    public void Spawn(Projectile prefab, Vector3 position, Quaternion rotation, Vector3 direction)
    {
        Projectile projectile = Instantiate(prefab, position, rotation);

        impactManager.Register(projectile);
        hitStatistics.Register(projectile);

        projectile.Launch(direction);
    }
}

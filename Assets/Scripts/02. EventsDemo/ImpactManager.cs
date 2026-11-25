using _02._EventsDemo;
using UnityEngine;

public class ImpactManager : MonoBehaviour
{

    [SerializeField] private ParticleSystem impactEffect;
    
    public void Register(Projectile projectile)
    {
        projectile.OnHit += HandleHit;
    }

    private void HandleHit(ProjectileHitData data)
    {
        if (data.Collision.gameObject.TryGetComponent<Target>(out Target target))
        {
            target.TakeDamage(data.Damage);
        }
        
        data.Projectile.OnHit -= HandleHit;
        SpawnImpactEffect(data.Collision);
        Destroy(data.Projectile.gameObject);
    }

    private void SpawnImpactEffect(Collision collision)
    {
        ContactPoint contactPoint = collision.GetContact(0);
        
        Instantiate(impactEffect, contactPoint.point, Quaternion.LookRotation(contactPoint.normal));
    }
}

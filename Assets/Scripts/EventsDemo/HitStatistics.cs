using _02._EventsDemo;
using UnityEngine;

public class HitStatistics : MonoBehaviour
{
    private int totalHits = 0;

    public void Register(Projectile projectile)
    {
        projectile.OnHit += HandleHit;
    }

    private void HandleHit(ProjectileHitData data)
    {
        totalHits++;
        Debug.Log($"Total hits: {totalHits}");

        data.Projectile.OnHit -= HandleHit;
    }
}

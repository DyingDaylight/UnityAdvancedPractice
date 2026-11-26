using UnityEngine;

namespace _02._EventsDemo
{
    public struct ProjectileHitData
    {
        public Projectile Projectile { get; }
        public Collision Collision { get; }
        public int Damage { get; }

        public ProjectileHitData(Projectile projectile, Collision collision, int damage)
        {
            Projectile = projectile;
            Collision = collision;
            Damage = damage;
        }
    }
}
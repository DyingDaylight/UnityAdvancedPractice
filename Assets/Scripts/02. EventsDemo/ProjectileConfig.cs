using UnityEngine;

namespace _02._EventsDemo
{
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "EventsDemo/Projectile Config", order = 0)]
    public class ProjectileConfig : ScriptableObject
    {
        [SerializeField] private int damage = 10;
        [SerializeField] private float speed = 15f;

        public int Damage => damage;
        public float Speed => speed;
    }
}
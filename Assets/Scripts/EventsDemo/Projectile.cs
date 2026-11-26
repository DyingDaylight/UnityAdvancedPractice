using System;
using _02._EventsDemo;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileConfig config;
    
    private Rigidbody rigidbody;
    
    public event Action<ProjectileHitData> OnHit;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
    
    public void Launch(Vector3 direction)
    {
        rigidbody.linearVelocity = config.Speed * direction.normalized;
    }
    
    private void OnCollisionEnter(Collision other)
    {
        ProjectileHitData data = new ProjectileHitData(this, other, config.Damage);
        OnHit?.Invoke(data);
    }
}

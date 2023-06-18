using UnityEngine;

namespace Assets.Scripts.Projectiles
{
  public class Arrow : Projectile
  {
    protected override void Init()
    {
    }

    protected override void OnHitEffect(Collider collision)
    {

      //Can only hit targets with a entity script attached
      if (collision.transform.parent != null && collision.transform.parent.TryGetComponent<Entity>(out var entity))
      {
        entity.TakeDamage(damage);
      }

      Destroy(gameObject);
      AudioSource.PlayClipAtPoint(hitSound, transform.position, 0.1f);
    }
  }
}

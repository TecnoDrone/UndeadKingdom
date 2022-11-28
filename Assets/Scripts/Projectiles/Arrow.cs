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
      if (!collision.transform.parent.TryGetComponent<Entity>(out var entity)) return;

      entity.TakeDamage(damage);
      AudioSource.PlayClipAtPoint(hitSound, transform.position, 0.1f);
      Destroy(gameObject);
    }
  }
}

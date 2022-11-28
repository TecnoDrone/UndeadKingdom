using UnityEngine;

namespace Assets.Scripts.Spells
{
  public class Fireball : SpellBehaviour
  {
    public float speed;
    public AudioClip hitSound;
    public GameObject fireHitEffect;

    private void Update()
    {
      transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    protected override void OnHitEffect(Collider collision)
    {
      var hits = Physics.SphereCastAll(transform.position, 1, transform.position);
      foreach (var hit in hits)
      {
        var entity = hit.transform.GetComponent<Entity>();
        if (entity != null)
        {
          entity.TakeDamage(damage);

          if (entity.IsDead)
          {
            var bodyparts = entity.Disintegrate();
            if (bodyparts != null && bodyparts.Count > 0)
            {
              foreach (var bodypart in bodyparts)
              {
                var rigidbody = bodypart.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                  rigidbody.AddExplosionForce(10f, transform.position, 1f, 3f, ForceMode.Impulse);
                }
              }
            }
          }
        }
      }

      //Explode - TODO: move explosion sound to fireHiteffect
      AudioSource.PlayClipAtPoint(hitSound, transform.position, 0.1f);

      var rotation = Quaternion.Euler(new Vector3(60, 0, transform.rotation.eulerAngles.z + 90));
      var explosion = Instantiate(fireHitEffect, transform.position, rotation);
      Destroy(explosion, 3);

      //Detroy fireball
      Destroy(gameObject);
    }
  }
}

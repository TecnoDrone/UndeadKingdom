using Assets.Scripts.Generic;
using UnityEngine;

namespace Assets.Scripts.Spells
{
  public class Fireball : Spell, ISpell
  {
    public float speed;
    public int damage;
    public AudioClip explosionSoundEffect;
    public GameObject fireHitEffect;

    private Vector3 startPosition;

    public override void Start()
    {
      base.Start();
      Destroy(gameObject, 3);
      startPosition = transform.position;

      //Use all energy
      damage = Player.Instance.energy;
      Player.Instance.UseEnergy(Player.Instance.energy);
    }

    void OnTriggerEnter(Collider collision)
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
      AudioSource.PlayClipAtPoint(explosionSoundEffect, transform.position, 0.1f);

      var rotation = Quaternion.Euler(new Vector3(60, 0, transform.rotation.eulerAngles.z + 90));
      var explosion = Instantiate(fireHitEffect, transform.position, rotation);
      Destroy(explosion, 3);

      //Detroy fireball
      Destroy(gameObject);
    }

    void Update()
    {
      transform.Translate(Vector3.up * speed * Time.deltaTime);
      transform.position = new Vector3(transform.position.x, startPosition.y, transform.position.z);
    }

    public override void StartingOrientation()
    {
      Vector3 toTarget = Destination - transform.position;
      float rotation = -Quaternion.LookRotation(Vector3.down, toTarget).eulerAngles.y;
      transform.rotation = Quaternion.Euler(60f, 0f, rotation);
    }
  }
}

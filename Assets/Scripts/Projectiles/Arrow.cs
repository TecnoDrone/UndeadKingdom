using UnityEngine;
using static Assets.Scripts.Squads;

namespace Assets.Scripts.Projectiles
{
  public class Arrow : MonoBehaviour
  {
    public int Damage;
    public float Speed;
    public AudioClip ArrowHitSoundEffect;
    public Squads squad;

    [HideInInspector]
    public Vector3 Destination;

    public SpriteRenderer sr;

    public void Start()
    {
      Destroy(gameObject, 3);
    }

    void Update()
    {
      transform.Translate(Vector3.up * Speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collision)
    {
      if (collision?.transform?.parent == null) return;

      if (collision.gameObject.layer == LayerMask.NameToLayer(squad.ToString())) return;
      
      if (collision.transform.parent.TryGetComponent<Entity>(out var entity))
      {
        entity.TakeDamage(Damage);
        AudioSource.PlayClipAtPoint(ArrowHitSoundEffect, transform.position, 0.1f);
        Destroy(gameObject);
      }
    }

    public void SetSquad(Squads squad)
    {
      this.squad = squad;
      if (sr == null) return;

      switch(this.squad)
      {
        case Undead: sr.color = Color.green; break;
        case GoodOnes: sr.color = Color.red; break;
      }
    }
  }
}

using Assets.Scripts.AI;
using UnityEngine;

namespace Assets.Scripts.Projectiles
{
  public abstract class Projectile : MonoBehaviour
  {
    public int damage;
    public float speed;
    public AudioClip hitSound;
    public Team team;

    [HideInInspector]
    public Vector3 destination;

    private SpriteRenderer outline;

    public void Start()
    {
      outline = transform.Find("Outline").GetComponent<SpriteRenderer>();
      Destroy(gameObject, 3);

      Init();
    }

    protected abstract void Init();

    protected abstract void OnHitEffect(Collider collision);

    void Update()
    {
      transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collision)
    {
      //Avoid Exceptions in case the parent gets killed
      if (collision?.transform?.parent == null) return;

      if (!collision.transform.parent.TryGetComponent<Entity>(out var entity)) return;

      //Avoid arrows to be blocked by allied
      if (entity.team == team) return;
      //if (collision.gameObject.layer == LayerMask.NameToLayer(team.ToString())) return;

      OnHitEffect(collision);
    }

    public void SetSquad(Team squad)
    {
      this.team = squad;
      if (outline == null) return;

      switch(this.team)
      {
        case Team.Undead: outline.color = Color.green; break;
        case Team.GoodOnes: outline.color = Color.red; break;
      }
    }
  }
}

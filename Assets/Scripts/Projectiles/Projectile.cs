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
      if (collision.tag == "corpse" || collision.tag == "projectile") return;
      if (collision.gameObject.layer == gameObject.layer) return;

      //Do not trigger with team members
      if (collision.transform.parent != null && collision.transform.parent.TryGetComponent<Entity>(out var entity))
      {
        if (entity.team == team) return;
      }

      OnHitEffect(collision);
    }

    public void SetSquad(Team team)
    {
      this.team = team;

      switch(this.team)
      {
        case Team.Undead:
          if (outline != null) outline.color = Color.green;
          break;
        case Team.GoodOnes:
          if (outline != null) outline.color = Color.red;
          break;
      }
      foreach (Transform child in transform) child.gameObject.layer = gameObject.layer;
    }
  }
}

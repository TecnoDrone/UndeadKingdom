using UnityEngine;

namespace Assets.Scripts.Spells
{
  public abstract class SpellBehaviour : MonoBehaviour
  {
    public int damage;
    public Team team;

    private SpriteRenderer outline;

    void Start()
    {
      outline = transform.Find("Outline")?.GetComponent<SpriteRenderer>();
      Destroy(gameObject, 3);
    }

    void OnTriggerEnter(Collider collider)
    {
      if (collider.gameObject.layer == LayerMask.NameToLayer(team.ToString())) return;

      OnHitEffect(collider);
    }

    public virtual bool CanCast() => true;

    public void SetSquad(Team team)
    {
      this.team = team;
      if (outline == null) return;

      switch (team)
      {
        case Team.Undead: outline.color = Color.green; break;
        case Team.GoodOnes: outline.color = Color.red; break;
      }
    }

    protected virtual void OnHitEffect(Collider collider) { }
  }
}

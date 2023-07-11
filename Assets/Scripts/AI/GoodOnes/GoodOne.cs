using UnityEngine;

namespace Assets.Scripts.AI.GoodOnes
{
  public class GoodOne : CombatAI
  {
    public override void Start()
    {
      base.Start();
      team = Team.GoodOnes;
    }

    public override void OnDeath(Entity entity)
    {
      base.OnDeath(entity);

      var corspe = spriteRenderer.gameObject.AddComponent<Corpse>();
      corspe.randomizeRotation = false;
      corspe.Reanimation = Resources.Load<GameObject>($"Prefabs/Creatures/Undead/Undead{Kind}/Undead{Kind}");

      var collider = spriteRenderer.gameObject.AddComponent<BoxCollider>();
      collider.size = new Vector3(1, 1, 0.1f);
      collider.center = new Vector3(0, 0, -collider.size.z / 2);

      spriteRenderer.gameObject.layer = LayerMask.NameToLayer("Corpse");
    }
  }
}

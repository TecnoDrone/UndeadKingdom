using Assets.Scripts.Managers;

namespace Assets.Scripts.AI.Undead.T1minions
{
  public class SkeletonKnight : Undead
  {
    public override void Start()
    {
      base.Start();

      attackRange = 1f;
      viewDistance = 3f;
      attackSpeed = 1f;
      Kind = CreatureKind.UndeadKnight;
      order = 1;
    }
  }
}

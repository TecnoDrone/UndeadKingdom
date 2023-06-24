using Assets.Scripts.Managers;

namespace Assets.Scripts.AI.Undead.T1minions
{
  public class SkeletonKnight : Undead
  {
    public override void Start()
    {
      base.Start();
      Kind = CreatureKind.UndeadKnight;
      order = 1;
    }
  }
}

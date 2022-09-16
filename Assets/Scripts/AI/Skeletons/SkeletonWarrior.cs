using Assets.Scripts.AI;

public class SkeletonWarrior : CombatAI
{
  public SkeletonWarrior()
  {
    Damage = 1;
    attackRange = 10f;
    viewDistance = 3f;
    attackSpeed = 1f;
  }
}

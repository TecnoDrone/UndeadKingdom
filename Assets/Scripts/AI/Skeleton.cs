using Assets.Scripts.AI;

public class Skeleton : CombatAI
{
  public Skeleton()
  {
    Damage = 1;
    attackRange = 10f;
    viewDistance = 3f;
    attackSpeed = 1f;
  }
}

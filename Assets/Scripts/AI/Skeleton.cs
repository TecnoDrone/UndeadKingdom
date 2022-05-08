using Assets.Scripts.AI;

public class Skeleton : FighterAI
{
  public Skeleton()
  {
    Damage = 1;
    findRange = 50f;
    attackRange = 10f;
    roamDistance = 3f;
    attackSpeed = 1f;
  }
}
